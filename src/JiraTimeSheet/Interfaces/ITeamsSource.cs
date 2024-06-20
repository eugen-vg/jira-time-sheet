using JiraTimeSheet.Model;

namespace JiraTimeSheet;

public interface ITeamsSource
{
	IEnumerable<WorklogRecord> GetWorklog(TeamsSettings settings, string chatName, DateOnly? startDate = null, DateOnly? endDate = null);
}