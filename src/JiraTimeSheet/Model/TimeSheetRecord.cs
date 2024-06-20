namespace JiraTimeSheet.Model;

public class TimeSheetRecord
{
	public DateTime Started { get; set; }
	public WorklogRecord JiraWorklogRecord { get; set; } = new();
	public WorklogRecord ExternalWorklogRecord { get; set; } = new();
}