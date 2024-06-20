using JiraTimeSheet;
using JiraTimeSheet.WorklogSources;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
	options.IdleTimeout = TimeSpan.FromSeconds(10);
	options.Cookie.HttpOnly = true;
	options.Cookie.IsEssential = true;
});
// Add services to the container.
builder.Services.AddRazorPages().AddJsonOptions(options =>
{
//	options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
	options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
	//options.JsonSerializerOptions.PropertyNamingPolicy = null;
//	options.JsonSerializerOptions.WriteIndented = true;
}).AddSessionStateTempDataProvider();

var graphUserScopes = builder.Configuration.GetSection("MsGraph:UserScopes").Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IDeviceCodeService, DeviceCodeService>();
builder.Services.AddTransient<IJiraSource, JiraSource>();
builder.Services.AddSingleton<ITeamsSource>(s => new TeamsSource(graphUserScopes, s.GetRequiredService<IDeviceCodeService>()));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();

/*
app.UseStaticFiles(new StaticFileOptions()
{
	FileProvider = new PhysicalFileProvider(
		Path.Combine(Directory.GetCurrentDirectory(), @"Images")),
	RequestPath = new PathString("/app-images")
});
*/
app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();