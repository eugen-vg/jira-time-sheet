using JiraTimeSheet.Model;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace JiraTimeSheet.Pages;

public class Create : PageModel
{
	public List<WorklogRecord> NewWorklog { get; set; } = new List<WorklogRecord>();
	public void OnGet(string Comment)
	{
		NewWorklog = JsonConvert.DeserializeObject<List<WorklogRecord>>(TempData[nameof(NewWorklog)]?.ToString() ?? string.Empty) ?? new List<WorklogRecord>();
		foreach (var worklogRecord in NewWorklog)
		{
			worklogRecord.Comment = Comment;
		}
	}
}