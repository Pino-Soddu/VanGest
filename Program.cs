using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.OpenApi.Models;
using VanGest.Server.Services; // Servizi personalizzati
using Blazored.LocalStorage;
using Radzen;
using System.Runtime.InteropServices;
using System.Net.Http.Headers;
using VanGest.Server.Components.Staff.Filters;
using VanGest.Server.Services.Auth;
using VanGest.Server.Services.Vans;
using VanGest.Server.Services.ARVans;
using VanGest.Server.Components.Staff.Map;

var builder = WebApplication.CreateBuilder(args);

if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    throw new PlatformNotSupportedException("Questa applicazione richiede Windows per l'accesso al database");
}

// Servizi Razor e Blazor
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor()
    .AddHubOptions(options =>
    {
        options.ClientTimeoutInterval = TimeSpan.FromMinutes(30);
        options.KeepAliveInterval = TimeSpan.FromSeconds(10);
    });
builder.Services.AddHttpContextAccessor();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddRadzenComponents();

builder.Services.AddScoped<GeocodingService>();
builder.Services.AddHttpClient<GeocodingService>();
builder.Services.AddHttpClient<GeocodingService>(client => {
    client.DefaultRequestHeaders.UserAgent.ParseAdd("YourApp/1.0");
    client.BaseAddress = new Uri("https://nominatim.openstreetmap.org/");
});

// Servizio di esportazione Excel
builder.Services.AddScoped<ExcelExportService>();

// HttpClient per API
builder.Services.AddHttpClient("VanApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7011/");
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

// Registrazione servizi specifici
builder.Services.AddScoped<IDeepSeekService, DeepSeekService>();
builder.Services.AddScoped<IARService, ARService>();
builder.Services.AddScoped<AdvancedFilter>();
builder.Services.AddScoped<IContextService, ContextService>();
builder.Services.AddScoped<ILocalitaDataManager, LocalitaDataManager>();

// Registrazione servizio per Google Maps
builder.Services.AddScoped<GoogleMapsOverlay>();
builder.Services.AddScoped<ModalService>();

// Registrazione servizi per l'accesso ai dati
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IVanService, VanService>();
builder.Services.AddScoped<IARVanService, ARVanService>();

// Swagger (opzionale)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "VanGest API",
        Version = "v1",
        Description = "API per l'area riservata"
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseCors();

app.UseAuthorization();

// Rimuovi se non usi più controller API
app.MapControllers();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();