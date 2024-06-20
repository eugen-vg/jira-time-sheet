namespace JiraTimeSheet.Model;

public class WorklogRecord
{
	public string Id { get; set; } = string.Empty;
	public Author? Author { get; set; }
	public DateTime? Created { get; set; }
	public DateTime? Started { get; set; }
	public string TimeSpent { get; set; } = string.Empty;
	public double TimeSpentSeconds { get; set; }
	public string Comment { get; set; } = string.Empty;
	public string Json { get; set; } = string.Empty;
}