#region Using

using FluentValidation.AspNetCore;
using MicroAutomation.Licensing.Api.Domain.Services;
using MicroAutomation.Licensing.Api.Service.Configurations;
using MicroAutomation.Licensing.Api.Service.Filters;
using MicroAutomation.Licensing.Api.Service.Helpers;
using MicroAutomation.Licensing.Data.Shared.DbContexts;
using MicroAutomation.Swagger.Extensions;
using MicroAutomation.Web.Extensions;
using MicroAutomation.WebAPI;
using MicroAutomation.WebAPI.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sieve.Models;
using Sieve.Services;
using System;
using System.Linq;
using System.Text.Json.Serialization;

#endregion Using

namespace MicroAutomation.Licensing.Api.Service
{
    public class Startup
    {
        /// <summary>
        /// Represents a set of key/value application configuration properties.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Startup
        /// </summary>
        /// <param name="configuration">Configuration</param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Configuration service.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            // Authentication
            services.AddJwtAuthentication(Configuration);

            // Frameworks
            services.AddEndpointsApiExplorer();
            services.AddSwagger(Configuration);

            // Add service for controllers.
            var controllers = services.AddControllers(opt => opt.Filters.Add<ApiExceptionFilterAttribute>());
            controllers.AddJsonOptions(opts => opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
            controllers.AddFluentValidation(s => s.RegisterValidatorsFromAssemblies(AppDomain.CurrentDomain.GetAssemblies().Where(p => !p.IsDynamic)));
            controllers.AddMvcOptions(opts => opts.SuppressAsyncSuffixInActionNames = false);

            // Frameworks
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.AddScoped<SieveProcessor>();
            services.AddHttpContextAccessor();

            // Sieve
            services.AddScoped<SieveProcessor>();
            services.AddScoped<ISieveProcessor, SieveProcessorConfigurator>();
            services.Configure<SieveOptions>(Configuration.GetSection("SieveConfiguration"));

            // Register configuration
            var productConfiguration = new LicensingConfiguration(Configuration);
            services.AddSingleton(productConfiguration);

            // Add DbContexts
            services.RegisterDbContexts<DataStoreDbContext, DataProtectionDbContext>(productConfiguration);
            services.AddDataProtection<DataProtectionDbContext>(productConfiguration);

            // Internal services
            services.AddTransient<ProductService>();
            services.AddTransient<LicenseService>();
        }

        /// <summary>
        /// Configure application.
        /// </summary>
        /// <param name="app">Application builder</param>
        /// <param name="env">Web host environment</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Checks if the current host environment name is development.
            if (env.IsDevelopment() || env.EnvironmentName == "LocalDevelopment")
                app.UseDeveloperExceptionPage();

            // Enables routing capabilities
            app.UseRouting();

            // Add wagger implementation
            app.UseSwaggerUI(Configuration);
            app.AddSwaggerRedirect();

            // Enable authentication and authorization
            app.UseAuthentication();
            app.UseAuthorization();

            // Endpoint Configuration
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}