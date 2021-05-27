using System;
using DevIO.Api.Extensions;
using Elmah.Io.AspNetCore;
using Elmah.Io.AspNetCore.HealthChecks;
using Elmah.Io.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DevIo.Api.Configuration
{
    public static class LoggerConfig
    {
        public static IServiceCollection AddLoggingConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddElmahIo(o =>
            {
                o.ApiKey = "9b6c90e596e94ad3893572f4f8004330";
                o.LogId = new Guid("db46ae17-89da-4b74-8e3a-3770ef8c6c53");
            });

            services.AddLogging(builder =>
            {
                builder.AddElmahIo(o =>
                {
                    o.ApiKey = "9b6c90e596e94ad3893572f4f8004330";
                    o.LogId = new Guid("db46ae17-89da-4b74-8e3a-3770ef8c6c53");
                });
                builder.AddFilter<ElmahIoLoggerProvider>(null, LogLevel.Warning);
            });

            services.AddHealthChecks()
                    .AddElmahIoPublisher(options =>
                    {
                        options.ApiKey = "9b6c90e596e94ad3893572f4f8004330";
                        options.LogId = new Guid("db46ae17-89da-4b74-8e3a-3770ef8c6c53");
                        options.HeartbeatId = "4aabbd81984d4b16865e07b8fd27132c";
                    })
                    .AddCheck("Produtos", new SqlServerHealthCheck(configuration.GetConnectionString("DefaultConnection")))
                    .AddSqlServer(configuration.GetConnectionString("DefaultConnection"), name: "BancoSQL");

            services.AddHealthChecksUI(s =>
            {
                s.AddHealthCheckEndpoint("endpoint1", "https://localhost:44318/health");
            })
                    .AddInMemoryStorage();

            return services;
        }

        public static IApplicationBuilder UseLoggingConfiguration(this IApplicationBuilder app)
        {
            app.UseElmahIo();

            return app;
        }
    }
}