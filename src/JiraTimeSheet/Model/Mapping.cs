namespace JiraTimeSheet.Model;

public class Mapping
{
	public string ChatName { get; set; } = string.Empty;
	public string Comment { get; set; } = string.Empty;
	public string JiraItem { get; set; } = string.Empty;
	public DateOnly StartDate { get; set; }
	public DateOnly EndDate { get; set; }	
}