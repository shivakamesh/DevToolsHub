using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using DevToolsHub;
using DevToolsHub.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<UserPrefs>();
builder.Services.AddScoped<ProLicense>();
builder.Services.AddScoped<DynamicQrStore>();
builder.Services.AddScoped<Microsoft.AspNetCore.Components.WebAssembly.Services.LazyAssemblyLoader>();

await builder.Build().RunAsync();
