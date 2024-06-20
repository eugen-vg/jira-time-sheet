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

	public Compare(IJiraSource jiraSource, ITeamsSource teamsSource)
	{
		_jiraSource = jiraSource;
		_teamsSource = teamsSource;
	}

	public void OnGet()
	{
		JiraSettings jiraSettings = new();
		TeamsSettings teamsSettings = new();
		List<Mapping> mappings = new();
		if (TempData["JiraConfig"] is string jiraConfigStr)
		{
			jiraSettings = JsonConvert.DeserializeObject<JiraSettings>(jiraConfigStr);
		}

		if (TempData["TeamsConfig"] is string teamsConfigStr)
		{
			teamsSettings = JsonConvert.DeserializeObject<TeamsSettings>(teamsConfigStr);
		}

		if (TempData["Mappings"] is string chatsStr)
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
}