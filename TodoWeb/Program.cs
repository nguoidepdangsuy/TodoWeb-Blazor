using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TodoWeb;
using TodoWeb.Services;
using Microsoft.Extensions.Configuration;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

Console.WriteLine("Starting Blazor WebAssembly application with localStorage...");

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Đọc configuration
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

// HttpClient cơ bản
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

// Đăng ký Supabase Service (tạm thời dùng localStorage)
builder.Services.AddScoped<SupabaseService>();

// Đăng ký các services - Sử dụng localStorage implementations
builder.Services.AddScoped<IAuthService, SupabaseAuthService>();
builder.Services.AddScoped<IGroupService, SupabaseGroupService>();
builder.Services.AddScoped<ITaskService, SupabaseTaskService>();
builder.Services.AddScoped<ITaskSubmissionService, SupabaseTaskSubmissionService>();
builder.Services.AddScoped<IUserTypeService, UserTypeService>();
builder.Services.AddScoped<IUserAuthorizationService, UserAuthorizationService>();

try
{
    Console.WriteLine("Building application...");
    var app = builder.Build();

    Console.WriteLine("Running application...");
    await app.RunAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"CRITICAL ERROR: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
    throw;
}