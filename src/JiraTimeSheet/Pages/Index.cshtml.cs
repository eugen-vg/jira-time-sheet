using JiraTimeSheet.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace JiraTimeSheet.Pages;

public class IndexModel : PageModel
{
	private readonly ILogger<IndexModel> _logger;
	private readonly ITeamsSource _teamsSource;

	[BindProperty] public JiraSettings JiraConfig { get; set; } = new();
	[BindProperty] public TeamsSettings TeamsConfig { get; set; } = new();

	public IndexModel(ILogger<IndexModel> logger, ITeamsSource teamsSource)
	{
		_logger = logger;
		_teamsSource = teamsSource;
	}

	public void OnGet()
	{
	}

	public IActionResult OnPost(List<Mapping> mappings)
	{
		TempData["JiraConfig"] = JsonConvert.SerializeObject(JiraConfig);
		TempData["TeamsConfig"] = JsonConvert.SerializeObject(TeamsConfig);
		TempData["Mappings"] = JsonConvert.SerializeObject(mappings);

		return RedirectToPage(nameof(Compare));
	}
}