using JiraTimeSheet;

public class DeviceCodeService : IDeviceCodeService
{
	private readonly ISession _session;
	private const string DeviceCodeKey = "DeviceCode";

	public DeviceCodeService(IHttpContextAccessor httpContextAccessor)
	{
		_session = httpContextAccessor.HttpContext.Session;
	}

	public void StoreDeviceCode(string code)
	{
		_session.SetString(DeviceCodeKey, code);
	}

	public string RetrieveDeviceCode()
	{
		var code = _session.GetString(DeviceCodeKey);
		_session.Remove(DeviceCodeKey);
		return code ?? string.Empty;
	}
}