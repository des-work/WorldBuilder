using Genisis.Core.Data;
using Genisis.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Windows;

namespace Genisis.App;

public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                ConfigureServices(services);
            })
            .Build();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // The path for our local database file
        var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GenisisAI", "genisis.db");
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

        // Register the DbContext for Dependency Injection
        services.AddDbContext<GenesisDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        // Register the Repository
        services.AddScoped<IUniverseRepository, UniverseRepository>();

        // Register the MainWindow
        services.AddSingleton<MainWindow>();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();

        // Get the DbContext instance and ensure the database is created
        var dbContext = _host.Services.GetRequiredService<GenesisDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);
    }
}