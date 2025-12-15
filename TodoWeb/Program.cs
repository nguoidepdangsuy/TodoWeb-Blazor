using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TodoWeb;
using TodoWeb.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

Console.WriteLine(" Starting Blazor WebAssembly application...");

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// HttpClient cơ bản
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

// QUAN TRỌNG: Đăng ký services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<ITaskService, TaskService>(); // THÊM DÒNG NÀY
builder.Services.AddScoped<ITaskSubmissionService, TaskSubmissionService>();
builder.Services.AddScoped<IUserTypeService, UserTypeService>();
builder.Services.AddScoped<IUserAuthorizationService, UserAuthorizationService>();

try
{
    Console.WriteLine(" Building application...");
    var app = builder.Build();

    Console.WriteLine(" Running application...");
    await app.RunAsync();
}
catch (Exception ex)
{
    Console.WriteLine($" CRITICAL ERROR: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
    throw;
}