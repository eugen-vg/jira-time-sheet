using JiraTimeSheet;
using Microsoft.Extensions.Caching.Memory;

public class DeviceCodeService : IDeviceCodeService
{
	private readonly IMemoryCache _cache;
	private readonly IHttpContextAccessor _httpContextAccessor;
	private const string DeviceCodeKey = "DeviceCode";

	public DeviceCodeService(IMemoryCache cache, IHttpContextAccessor httpContextAccessor)
	{
		_cache = cache;
		_httpContextAccessor = httpContextAccessor;
	}

	private string GetCacheKey()
	{
		// Use session ID if available, otherwise use a connection identifier
		var sessionId = _httpContextAccessor.HttpContext?.Session?.Id;
		if (!string.IsNullOrEmpty(sessionId))
		{
			return $"{DeviceCodeKey}_{sessionId}";
		}

		// Fallback: use connection ID or a default key
		var connectionId = _httpContextAccessor.HttpContext?.Connection?.Id;
		return $"{DeviceCodeKey}_{connectionId ?? "default"}";
	}

	public void StoreDeviceCode(string code)
	{
		var cacheKey = GetCacheKey();
		// Store with a reasonable expiration (5 minutes should be enough for device code flow)
		_cache.Set(cacheKey, code, TimeSpan.FromMinutes(5));
	}

	public string RetrieveDeviceCode()
	{
		var cacheKey = GetCacheKey();
		if (_cache.TryGetValue(cacheKey, out string code))
		{
			_cache.Remove(cacheKey);
			return code ?? string.Empty;
		}
		return string.Empty;
	}
}