namespace JiraTimeSheet.Model;

public class GetWorklogResponse
{
	public int StartAt { get; set; }
	public int MaxResults { get; set; }
	public int Total { get; set; }
	public List<WorklogRecord> Worklogs { get; set; } = new();
}