using JiraTimeSheet.Model;

namespace JiraTimeSheet;

public interface  IJiraSource
{
	IEnumerable<WorklogRecord> GetWorklog(JiraSettings settings, string jiraItem, DateOnly? startDate = null, DateOnly? endDate = null);
	Task<bool> CreateWorklog(JiraSettings settings, string jiraItem, string worklogJson);
}