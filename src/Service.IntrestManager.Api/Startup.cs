using System.Reflection;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyJetWallet.Sdk.GrpcSchema;
using MyJetWallet.Sdk.Postgres;
using MyJetWallet.Sdk.Service;
using Prometheus;
using Service.InterestManager.Postrges;
using Service.IntrestManager.Api.Modules;
using Service.IntrestManager.Api.Services;
using Service.IntrestManager.Grpc;
using SimpleTrading.ServiceStatusReporterConnector;

namespace Service.IntrestManager.Api
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.BindCodeFirstGrpc();
            
            services.AddHostedService<ApplicationLifetimeManager>();
            services.AddMyTelemetry("SP-", Program.Settings.ZipkinUrl);
            
            services.AddDatabaseWithoutMigrations<DatabaseContext>(DatabaseContext.Schema, Program.Settings.PostgresConnectionString);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseMetricServer();

            app.BindServicesTree(Assembly.GetExecutingAssembly());

            app.BindIsAlive();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcSchema<InterestRateSettingsService, IInterestRateSettingsService>();
                endpoints.MapGrpcSchema<InterestManagerConfigService, IInterestManagerConfigService>();
                endpoints.MapGrpcSchema<InterestManagerService, IInterestManagerService>();
                endpoints.MapGrpcSchema<InterestRateClientService, IInterestRateClientService>();
                
                endpoints.MapGrpcSchemaRegistry();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterModule<SettingsModule>();
            builder.RegisterModule<ServiceModule>();
            builder.RegisterModule<ClientModule>();
        }
    }
}
