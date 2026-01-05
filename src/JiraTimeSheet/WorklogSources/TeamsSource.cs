using Azure.Identity;
using JiraTimeSheet.Model;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace JiraTimeSheet.WorklogSources;

public class TeamsSource : ITeamsSource
{
	private readonly string[] _scopes;
	private readonly IDeviceCodeService _deviceCodeService;
	private DeviceCodeCredential? _deviceCodeCredential;

	public TeamsSource(string[] scopes, IDeviceCodeService deviceCodeService)
	{
		_scopes = scopes;
		_deviceCodeService = deviceCodeService;
	}

	public async Task<IEnumerable<WorklogRecord>> GetWorklog(TeamsSettings settings, string chatName, DateOnly? startDate = null, DateOnly? endDate = null)
	{
		var options = new DeviceCodeCredentialOptions
		{
			ClientId = settings.ClientId,
			TenantId = settings.TenantId,
			DeviceCodeCallback = async (deviceCodeInfo, cancellationToken) =>
			{
				_deviceCodeService.StoreDeviceCode(deviceCodeInfo.Message);
				await Task.FromResult(0);
			}
		};
		_deviceCodeCredential = new DeviceCodeCredential(options);
		GraphServiceClient userClient = new GraphServiceClient(_deviceCodeCredential, _scopes);
		var user = await userClient.Me.GetAsync((config) => { config.QueryParameters.Select = new[] { "displayName", "mail", "userPrincipalName" }; });
		//if (user.Mail != userEmail) throw new Exception("MsTeamsSource user not found");
		var chats = await userClient.Chats.GetAsync(
			requestConfiguration =>
			{
				requestConfiguration.QueryParameters.Filter = $"topic eq '{chatName}'";
				requestConfiguration.QueryParameters.Select = new[] { "id", "topic" };
			}
			);
		if (chats == null) throw new Exception("Chat not found");
		Chat? chat = null;
		var chatIterator = PageIterator<Chat, ChatCollectionResponse>.CreatePageIterator(
				userClient,
				chats,
				chatResult =>
				{
					if (chatResult.Topic == chatName) chat = chatResult;
					return true;
				}
				);
		await chatIterator.IterateAsync();
		if (chat == null) throw new Exception("Chat not found");
		var worklogRecords = new List<WorklogRecord>();
		var messages = await userClient.Chats[chat.Id].Messages.GetAsync(
			requestConfiguration =>
			{
				if (startDate == null || endDate == null) return;
				requestConfiguration.QueryParameters.Orderby = new[] { "lastModifiedDateTime desc" };
				requestConfiguration.QueryParameters.Filter = $"lastModifiedDateTime gt {startDate.Value.ToString("yyyy-MM-dd") + "T00:00:00.000Z"} and lastModifiedDateTime lt {endDate.Value.ToString("yyyy-MM-dd") + "T23:59:59.999Z"}";
			}
		);
		if (messages == null) return new List<WorklogRecord>();

		var pageIterator = PageIterator<ChatMessage, ChatMessageCollectionResponse>.CreatePageIterator(
			userClient,
			messages,
			(msg) =>
			{
				if (msg.EventDetail is not CallEndedEventMessageDetail detail) return true;
				if (!(detail.CallParticipants?.Any(p => p.Participant?.User?.DisplayName == user?.DisplayName) ?? false)) return true;

				var timeSecs = detail.CallDuration?.TotalSeconds ?? 0;
				worklogRecords.Add(new WorklogRecord
				{
					Started = msg.CreatedDateTime?.DateTime,
					TimeSpent = WorklogFormatter.ConvertSecondsToFormattedTime(timeSecs),
					TimeSpentSeconds = timeSecs
				});
				return true;
			});
		await pageIterator.IterateAsync();

		return WorklogFormatter.GroupWorklogByStartedDate(worklogRecords);
	}
}