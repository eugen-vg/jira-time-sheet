using JiraTimeSheet.Model;
using JiraTimeSheet.WorklogSources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace JiraTimeSheet.Pages;

public class Compare : PageModel
{
	[BindProperty] public List<WorklogRecord> JiraWorklog { get; set; } = new();
	[BindProperty] public Dictionary<string, List<TimeSheetRecord>> TimeSheet { get; set; } = new();
	[BindProperty] public List<WorklogRecord> TeamsWorklog { get; set; } = new();

	private readonly IJiraSource _jiraSource;
	private readonly ITeamsSource _teamsSource;
	private readonly ILogger<Compare> _logger;

	public Compare(IJiraSource jiraSource, ITeamsSource teamsSource, ILogger<Compare> logger)
	{
		_jiraSource = jiraSource;
		_teamsSource = teamsSource;
		_logger = logger;
	}

	public void OnGet()
	{
		JiraSettings jiraSettings = new();
		TeamsSettings teamsSettings = new();
		List<Mapping> mappings = new();
		
		if (TempData.Peek("JiraConfig") is string jiraConfigStr)
		{
			jiraSettings = JsonConvert.DeserializeObject<JiraSettings>(jiraConfigStr);
		}

		if (TempData.Peek("TeamsConfig") is string teamsConfigStr)
		{
			teamsSettings = JsonConvert.DeserializeObject<TeamsSettings>(teamsConfigStr);
		}

		if (TempData.Peek("Mappings") is string chatsStr)
		{
			mappings = JsonConvert.DeserializeObject<List<Mapping>>(chatsStr);
		}

		JiraWorklog = _jiraSource.GetWorklog(jiraSettings, mappings[0].JiraItem).ToList();
		foreach (var mapping in mappings)
		{
			var worklog = _teamsSource.GetWorklog(teamsSettings, mapping.ChatName, mapping.StartDate, mapping.EndDate).ToList();
			foreach (var worklogRecord in worklog)
			{
				worklogRecord.Comment = mapping.Comment;
			}

			TeamsWorklog.AddRange(worklog);
		}

		TeamsWorklog = WorklogFormatter.GroupWorklogByStartedDate(TeamsWorklog).ToList();
		TimeSheet = new Dictionary<string, List<TimeSheetRecord>>()
		{
			{ mappings[0].JiraItem, WorklogFormatter.MergeWorklogs(JiraWorklog, TeamsWorklog).ToList() }
		};
		TempData[nameof(JiraWorklog)] = JsonConvert.SerializeObject(JiraWorklog);
		TempData[nameof(TeamsWorklog)] = JsonConvert.SerializeObject(TeamsWorklog);
	}

	public async Task<IActionResult> OnPostCreateWorklogAsync([FromBody] CreateWorklogRequest request)
	{
		try
		{
			JiraSettings jiraSettings = new();
			List<Mapping> mappings = new();

			if (TempData.Peek("JiraConfig") is string jiraConfigStr)
			{
				jiraSettings = JsonConvert.DeserializeObject<JiraSettings>(jiraConfigStr) ?? new JiraSettings();
			}

			if (TempData.Peek("Mappings") is string mappingsStr)
			{
				mappings = JsonConvert.DeserializeObject<List<Mapping>>(mappingsStr) ?? new List<Mapping>();
			}

			if (mappings.Count == 0)
			{
				_logger.LogWarning("No mappings found in TempData");
				return new JsonResult(new { success = false, message = "No mappings found. Please refresh the page and try again." });
			}

			var jiraItem = mappings[0].JiraItem;
			_logger.LogInformation($"Creating worklog for Jira item: {jiraItem}");
			var success = await _jiraSource.CreateWorklog(jiraSettings, jiraItem, request.WorklogJson);

			return new JsonResult(new { success = success, message = success ? "Worklog created successfully" : "Failed to create worklog" });
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error creating worklog");
			return new JsonResult(new { success = false, message = $"Error: {ex.Message}" });
		}
	}
}

public class CreateWorklogRequest
{
	public string WorklogJson { get; set; } = string.Empty;
}