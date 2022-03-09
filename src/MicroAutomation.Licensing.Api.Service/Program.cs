using MicroAutomation.Log.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Reflection;

namespace MicroAutomation.Licensing.Api.Service;

public class Program
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="args"></param>
    public static void Main(string[] args)
    {
        CreateHostBuilder(args)
            .Build().Run();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args);

        // Set current directory
        var location = new Uri(Assembly.GetEntryAssembly().GetName().CodeBase);
        var pathContextRoot = new FileInfo(location.AbsolutePath).Directory.FullName;
        builder.UseContentRoot(pathContextRoot);
        Directory.SetCurrentDirectory(pathContextRoot);

        // Add serilog implementation
        builder.UseCustomSerilog();

        // Sets up the configuration for the remainder of the build process and application
        builder.ConfigureAppConfiguration((hostContext, config) =>
        {
            // Retrieve the name of the environment
            var aspnetcore = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var dotnetcore = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
            var environmentName = string.IsNullOrWhiteSpace(aspnetcore) ? dotnetcore : aspnetcore;
            if (string.IsNullOrWhiteSpace(environmentName))
                environmentName = "Production";

            // Define the configuration builder
            config.SetBasePath(pathContextRoot);
            config.AddJsonFile("appsettings.json", optional: false);
            config.AddJsonFile($"appsettings.{environmentName}.json", optional: true);
            config.AddEnvironmentVariables();
            config.AddCommandLine(args);
        });

        // Configures a HostBuilder with defaults for hosting a web app.
        builder.ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        });

        return builder;
    }
}