using Microsoft.EntityFrameworkCore;
using TableTennisHistoric;
using TableTennisHistoric.Datas;
using TableTennisHistoric.Services;
using System.Diagnostics;
using TableTennisHistoric.Services.Interfaces;

// ---------- Builder ----------
var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls(builder.Configuration["AppUrl"] ?? "http://localhost:44304");

// ---------- Services ----------
builder.Services.AddDbContext<TableTennisHistoricDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 31))
    )
);

// Contrôleurs API
builder.Services.AddControllers();
builder.Services.AddControllersWithViews(options =>
{
    options.ModelBinderProviders.Insert(0, new DecimalModelBinderProvider());
});

// Pages Razor
builder.Services.AddRazorPages()
       .AddMvcOptions(options =>
       {
           options.ModelBinderProviders.Insert(0, new DecimalModelBinderProvider());
       });

// Services métiers
builder.Services.AddScoped<ICompetitionService, CompetitionService>();
builder.Services.AddScoped<IMatchService, MatchService>();
builder.Services.AddScoped<IClubService, ClubService>();
builder.Services.AddScoped<IMatchSetService, MatchSetService>();
builder.Services.AddScoped<IPlayerService, PlayerService>();
builder.Services.AddScoped<ISeasonService, SeasonService>();
builder.Services.AddScoped<IChampionshipService, ChampionshipService>();

// Session
builder.Services.AddDistributedMemoryCache(); // stockage en mémoire pour la session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.IsEssential = true;
});

// Swagger pour dev
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ---------- App ----------
var app = builder.Build();

// ---------- Middleware ----------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

// ---------- Routes ----------
app.MapControllers();
app.MapRazorPages();

// ---------- Ouverture automatique du navigateur ----------
void OpenBrowser()
{
    try
    {
        var url = app.Configuration["AppUrl"] ?? "http://localhost:44304"; // par défaut HTTP
        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }
    catch
    {
        // Ignorer si le navigateur ne s'ouvre pas
    }
}

// Hors mode Debug, on lance le navigateur juste après que Kestrel démarre
if (!app.Environment.IsDevelopment())
{
    app.Lifetime.ApplicationStarted.Register(() =>
    {
        Task.Run(async () =>
        {
            await Task.Delay(1000);

            try
            {
                var url = app.Configuration["AppUrl"] ?? "http://localhost:44304";
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch { }
        });
    });
}

// ---------- Lancement de Kestrel ----------
var runTask = app.RunAsync();

// ---------- Fermeture propre avec Ctrl+C ----------
Console.WriteLine("Application démarrée. Appuyez sur Ctrl+C pour quitter...");
Console.CancelKeyPress += (sender, e) =>
{
    Console.WriteLine("Arrêt de l'application...");
    app.StopAsync().Wait();
    Environment.Exit(0);
};

// On bloque ici pour que le programme reste vivant
runTask.Wait();


