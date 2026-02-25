/*
    Tobasa OpenJKN Bridge
    Copyright (C) 2020-2026 Jefri Sibarani
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using System;

// Dev only
//using Microsoft.Extensions.Hosting;
//using Microsoft.OpenApi.Models;
//using System.Collections.Generic;

using System.Linq;
using Tobasa.App;
using Tobasa.Data;
using Tobasa.Entities;
using Tobasa.Services;

namespace Tobasa
{
    public class Startup
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration      _configuration;

        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            _env = env;
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Apply Kestrel configurations
            services.Configure<KestrelServerOptions>(_configuration.GetSection("Kestrel"));

            // Configure AppSettings in DI
            services.Configure<AppSettings>(_configuration.GetSection("AppSettings"));

            // load Database datasources
            /// NOTE:https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.entityframeworkservicecollectionextensions.adddbcontext?view=efcore-3.1
            // Data context for this webservice core data
            // default to use SQLite datasource
            if (_configuration["DatabaseEngine"] == "MSSQL" ) {
                services.AddDbContext<DataContextAntrian>();
            }
            else if (_configuration["DatabaseEngine"] == "MYSQL" ) {
                services.AddDbContext<DataContextAntrian, DataContextAntrianMySql>();
            }
            else if (_configuration["DatabaseEngine"] == "SQLITE") { 
                services.AddDbContext<DataContextAntrian, DataContextAntrianSqlite>();
            }
            else if (_configuration["DatabaseEngine"] == "PGSQL") {
                services.AddDbContext<DataContextAntrian, DataContextAntrianPostgreSql>();
            }
            else {
                throw new Exception("Unsupported database engine: " + _configuration["DatabaseEngine"]);
            }

            services.AddCors();
            services.AddControllers()
                    .ConfigureApiBehaviorOptions(options =>
                    {
                        // Customize The Model Validation Response

                        // Note: https://docs.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-3.1#automatic-http-400-responses
                        options.SuppressMapClientErrors = true;

                        //options.InvalidModelStateResponseFactory = (context => new ApiNegativeResult("Invalid JSON model state"));
                        options.InvalidModelStateResponseFactory = context =>
                        {
                            // Note: https://docs.microsoft.com/en-us/aspnet/core/web-api/handle-errors?view=aspnetcore-5.0
                            // Note: https://stackoverflow.com/questions/62758019/how-to-make-one-or-more-validation-errors-occurred-raise-an-exception
                            // Note: https://kevsoft.net/2020/02/09/adding-errors-to-model-state-and-returning-bad-request-within-asp-net-core-3-1.html
                            // Note: https://stackoverflow.com/a/59211386
                            var errorMessage = "";
                            var errorsInModelState = context.ModelState
                                .Where(x => x.Value.Errors.Count > 0)
                                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Errors.Select(x => x.ErrorMessage).ToArray());

                            foreach (var error in errorsInModelState)
                            {
                                if (!string.IsNullOrWhiteSpace(errorMessage)) {
                                    errorMessage += ", ";
                                }

                                foreach (var subError in error.Value)
                                {
                                    errorMessage += "[" + error.Key + "] " + subError;
                                }
                            }
                            //errorMessage = JsonSerializer.Serialize(errorsInModelState, new JsonSerializerOptions { WriteIndented = true });
                            var result = new ApiNegativeResult("Validation error: " + errorMessage, 201, 400);
                            return result;
                        };

                    });

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            // configure strongly typed settings objects
            var appSettingsSection = _configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);

            services.AddHttpContextAccessor();

            // configure DI for application services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IMjknService, MjknService>();
            services.AddScoped<IMjknServiceFKTP, MjknServiceFKTP>();
            services.AddScoped<IVClaimApiService, VClaimApiService>();
            services.AddScoped<DBMigration>();

            /*
            if (_env.IsDevelopment())
            {
                services.AddSwaggerDocumentation();
            }
            */
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, DataContextAntrian dataContext, ILogger<Startup> logger)
        {
            app.UseStatusCodePagesWithReExecute("/error", "?statusCode={0}");


            // ----------------------------------------------------------------
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var migration = scope.ServiceProvider.GetRequiredService<DBMigration>();
                bool dbOk = migration.InitializeDatabase(dataContext);
                if (dbOk)
                {
                    migration.CreateDefaultUser();
                }
                else
                {
                    throw new Exception("Database initialization failed. Please check the logs for more details.");
                }
            }
            // ----------------------------------------------------------------

            app.UseRouting();

            // global cors policy
            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseMiddleware<RequestResponseLoggingMiddleware>();
            // since we use custom jwt auth middleware, we don't need app.UseAuthentication() and  app.UseAuthorization()
            // custom jwt auth middleware
            app.UseMiddleware<JwtMiddleware>();
            app.UseMiddleware<ExceptionHandlerMiddleware>();

            app.UseEndpoints(endpoints => endpoints.MapControllers());

/*
            if (env.IsDevelopment())
            {
                app.UseSwaggerDocumentation();
            }
*/

            Console.WriteLine($"* ======================================================================");
            Console.WriteLine($"* Web Service Mobile BPJS, Version {BuildInfo.Version} Build: {BuildInfo.BuildDate}");
            Console.WriteLine($"* Copyright 2026 Jefri Sibarani - Mangapul");
            Console.WriteLine($"* ======================================================================");
            Console.WriteLine("");
            logger.LogInformation($"Web Service Mobile BPJS, Version {BuildInfo.Version} Build: {BuildInfo.BuildDate} started.");
        }
    }

/*
    internal static class SwaggerServiceExtensions
    {
        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1.0", new OpenApiInfo { Title = "Main API v1.0", Version = "v1.0" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id   = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name   = "Bearer",
                            In     = ParameterLocation.Header,
                        },
                        new List<string>()
                    }
                });
            });

            return services;
        }

        public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1.0/swagger.json", "SIMRS API v1.0");
                c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
            });

            return app;
        }
    }
*/
}