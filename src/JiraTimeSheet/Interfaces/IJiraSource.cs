using JiraTimeSheet.Model;

namespace JiraTimeSheet;

public interface  IJiraSource
{
	IEnumerable<WorklogRecord> GetWorklog(JiraSettings settings, string jiraItem, DateOnly? startDate = null, DateOnly? endDate = null);
}