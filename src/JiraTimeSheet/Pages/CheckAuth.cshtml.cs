using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JiraTimeSheet.Pages;

public class CheckAuth : PageModel
{
	private readonly IDeviceCodeService _deviceCodeService;

	public CheckAuth(IDeviceCodeService deviceCodeService)
	{
		_deviceCodeService = deviceCodeService;
	}

	public JsonResult OnGet()
	{
		string code = _deviceCodeService.RetrieveDeviceCode();
		if (!string.IsNullOrEmpty(code))
		{
			code = code.Replace("https://microsoft.com/devicelogin", @"<a href=""https://microsoft.com/devicelogin"" target=""_blank"">https://microsoft.com/devicelogin</a>");
		}

		return new JsonResult(code);
	}
}