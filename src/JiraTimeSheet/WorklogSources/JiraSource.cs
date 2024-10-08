using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using JiraTimeSheet.Model;

namespace JiraTimeSheet.WorklogSources;

public class JiraSource : IJiraSource
{
	private class CustomDateTimeConverter : JsonConverter<DateTime>
	{
		private const string Format = "yyyy-MM-ddTHH:mm:ss.fffzzz";

		public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var str = reader.GetString() ?? string.Empty;
			return DateTime.ParseExact(str, Format, CultureInfo.InvariantCulture);
		}

		public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.ToString(Format));
		}
	}

	public IEnumerable<WorklogRecord> GetWorklog(JiraSettings settings, string jiraItem, DateOnly? startDate = null, DateOnly? endDate = null)
	{
		HttpClient client = new();
		client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
			"Basic",
			Convert.ToBase64String(Encoding.ASCII.GetBytes($"{settings.User}:{settings.Password}")));
		var userName = settings.User;
		if (userName.Contains('@'))
		{
			userName = userName.Substring(0, userName.IndexOf('@'));
		}
		var options = new JsonSerializerOptions();
		options.Converters.Add(new CustomDateTimeConverter());
		options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

		var response = client.GetAsync(new Uri(new Uri(settings.Url), "rest/api/2/myself")).Result;
		response.EnsureSuccessStatusCode();
		var responseContent = response.Content.ReadAsStringAsync().Result;
		var user = JsonSerializer.Deserialize<JiraUser>(responseContent, options) ?? new JiraUser();

		response = client.GetAsync(new Uri(new Uri(settings.Url), $"rest/api/2/issue/{jiraItem}/worklog")).Result;
		response.EnsureSuccessStatusCode();
		responseContent = response.Content.ReadAsStringAsync().Result;
		GetWorklogResponse worklogResponse = JsonSerializer.Deserialize<GetWorklogResponse>(responseContent, options) ?? new GetWorklogResponse();
		var userWorklog = worklogResponse.Worklogs.Where(w => w.Author?.EmailAddress == user.EmailAddress).ToList();

		return WorklogFormatter.GroupWorklogByStartedDate(userWorklog);
	}
}

public class JiraUser
{
	public string Self { get; set; } = string.Empty;
	public string EmailAddress { get; set; } = string.Empty;
}