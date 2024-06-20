using JiraTimeSheet.Model;
using Newtonsoft.Json;

namespace JiraTimeSheet.WorklogSources;

public static class WorklogFormatter
{
	public static string ConvertSecondsToFormattedTime(double totalSeconds)
	{
		double totalMinutes = totalSeconds / 60.0;
		int roundedMinutes = (int)Math.Round(totalMinutes / 5.0) * 5;
		int hours = roundedMinutes / 60;
		int minutes = roundedMinutes % 60;
		var formattedTime = hours > 0 ? $"{hours}h {minutes}m" : $"{minutes}m";

		return formattedTime;
	}

	public static IEnumerable<WorklogRecord> GroupWorklogByStartedDate(List<WorklogRecord> worklog)
	{
		var groupedWorklogs = worklog
			.Where(w => w.Started.HasValue)
			.GroupBy(w => w.Started!.Value.Date)
			.Select(g =>
			{
				var record = new WorklogRecord
				{
					Started = g.Max(w => w.Started),
					TimeSpentSeconds = g.Sum(w => w.TimeSpentSeconds),
				};

				record.TimeSpent = ConvertSecondsToFormattedTime(record.TimeSpentSeconds);

				if (g.Count() > 1)
				{
					record.Comment = g.Aggregate(string.Empty, (current, w) => current + ($"{w.TimeSpent}({w.Comment}); "));
				}
				else
				{
					record.Comment = g.First().Comment;
				}

				record.Json = WorklogFormatter.GetCreateJson(record);
				return record;
			})
			.OrderBy(r => r.Started)
			.ToList();

		return groupedWorklogs;
	}

	public static IEnumerable<TimeSheetRecord> MergeWorklogs(List<WorklogRecord> jiraWorklog, List<WorklogRecord> fileWorklog)
	{
		var dict = new Dictionary<DateTime, TimeSheetRecord>();

		foreach (var worklog in jiraWorklog)
		{
			if (!worklog.Started.HasValue) continue;
			var dateKey = worklog.Started.Value.Date;
			dict[dateKey] = new TimeSheetRecord
			{
				Started = dateKey,
				JiraWorklogRecord = worklog
			};
		}

		foreach (var worklog in fileWorklog)
		{
			if (!worklog.Started.HasValue) continue;
			var dateKey = worklog.Started.Value.Date;
			if (dict.TryGetValue(dateKey, out var timeSheetRecord))
			{
				timeSheetRecord.ExternalWorklogRecord = worklog;
			}
			else
			{
				dict[dateKey] = new TimeSheetRecord
				{
					Started = dateKey,
					ExternalWorklogRecord = worklog
				};
			}
		}

		var timeSheet = new List<TimeSheetRecord>(dict.Values).OrderBy(r => r.Started).ToList();
		return timeSheet;
	}

	private static string GetCreateJson(WorklogRecord record)
	{
		var json = JsonConvert.SerializeObject(new
		{
			started = record.Started?.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz").Remove(record.Started.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz").LastIndexOf(':'), 1),
			timeSpent = record.TimeSpent,
			comment = record.Comment
		}, Formatting.Indented);
		return json;
	}
}