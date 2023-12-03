using CustomBackend.Infra.Settings;
using CustomBackend.Infra.Utils;
using CustomBackend.Repository.Db.Context;
using CustomBackend.Repository.Db.Interceptors;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

namespace CustomBackend.Api.Infra
{
    public static class ConfigurationHelper
    {
        public static AppSettings SettingSetup(this WebApplicationBuilder builder)
        {
            var result = builder.Configuration.Get<AppSettings>();
            return result;
        }



        public static IServiceCollection AddCustomAuthentication(this IServiceCollection services, AppSettings settings)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ClockSkew = TimeSpan.FromMinutes(1),

                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(settings.JwtSetting.Secret)),
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateActor = false
                };
            });

            return services;
        }

        public static IServiceCollection AddCustomControllers(this IServiceCollection services)
        {
            // -> format enums as strings
            services
                .AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                });

            return services;
        }

        public static IServiceCollection AddCustomApiVersioning(this IServiceCollection services)
        {
            services.AddApiVersioning(p =>
            {
                p.DefaultApiVersion = new ApiVersion(1, 0);
                p.ReportApiVersions = true; // -> api version info in the header 
                p.AssumeDefaultVersionWhenUnspecified = true;
            });

            services.AddVersionedApiExplorer(p =>
            {
                p.GroupNameFormat = "'v'VVV";
                p.SubstituteApiVersionInUrl = true;
            });

            return services;
        }

        public static IServiceCollection AddCustomSwaggerDocGenForApiVersioning(this IServiceCollection services, string appName, string appVersionName)
        {
            services.AddEndpointsApiExplorer();

            // -> "compile" the IoC container to get the api version list from provider.
            var provider = services.BuildServiceProvider();
            var apiVersionDescriptionProvider = provider.GetRequiredService<IApiVersionDescriptionProvider>();

            services.AddSwaggerGen(options =>
            {
                foreach (var item in apiVersionDescriptionProvider.ApiVersionDescriptions)
                    options.SwaggerDoc(item.GroupName, new OpenApiInfo { Title = appVersionName, Version = item.ApiVersion.ToString() });

                // -> Get the comments from controllers actions to swagger use them
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{appName}.xml"));

                // -> Bearer Auth setup
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header. Example: \"Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme{
                            Reference = new OpenApiReference{
                                Id = "Bearer",
                                Type = ReferenceType.SecurityScheme
                            }
                        },new List<string>()
                    }
                });
            });

            return services;
        }

        public static IServiceCollection AddSqlServerDb(this IServiceCollection services, AppSettings settings)
        {
            services.AddDbContextPool<MainDbContext>((serviceProvider, options) =>
                options
                    .UseLazyLoadingProxies()
                    .UseSqlServer(settings.MainDbConnection)
                    .EnableSensitiveDataLogging()
                    .ConfigureWarnings(warnings => warnings.Ignore(CoreEventId.LazyLoadOnDisposedContextWarning))
                    .AddInterceptors(serviceProvider.GetRequiredService<UpdateAuditableInterceptor>())
            );

            return services;
        }

        public static IServiceCollection AddScopedByBaseType<TBase>(this IServiceCollection services, Assembly assembly = null)
        {
            ReflectionUtil.ListTypesOf<TBase>(assembly).ForEach(type =>
            {
                var i = type.GetInterface($"I{type.Name}");

                if (i == null)
                    services.AddScoped(type);
                else
                    services.AddScoped(i, type);
            }
            );

            return services;
        }



        public static WebApplication UseControllersEndpoints(this WebApplication app)
        {
            app.MapControllers();

            return app;
        }

        public static IApplicationBuilder UseCustomCors(this IApplicationBuilder app)
        {
            app.UseCors(builder =>
            {
                builder.AllowAnyHeader();
                builder.AllowAnyMethod();
                builder.AllowAnyOrigin();
            });

            return app;
        }

        public static IApplicationBuilder UseCustomSwagger(this IApplicationBuilder app, IApiVersionDescriptionProvider provider)
        {
            app.UseSwagger();

            app.UseSwaggerUI(options =>
            {
                // -> helps to handle with the Areas, case exists
                var swaggerJsonBasePath = string.IsNullOrWhiteSpace(options.RoutePrefix) ? "." : "..";

                var versions = provider.ApiVersionDescriptions.ToList();
                versions.Reverse();

                // -> drop down list for the doc spec
                foreach (var description in versions)
                    options.SwaggerEndpoint($"{swaggerJsonBasePath}/swagger/{description.GroupName}/swagger.json", description.GroupName);

                options.DocExpansion(DocExpansion.List);
            });

            return app;
        }

        public static IApplicationBuilder UseDatabaseInitialization(this IApplicationBuilder app, ILogger log, AppSettings settings)
        {
            if (settings.UpdateDatabase)
            {
                using (var scope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
                {
                    try
                    {
                        scope.ServiceProvider.GetRequiredService<MainDbContext>().Database.Migrate();
                    }
                    catch (Exception ex)
                    {
                        log.LogError(ex, $"Entity Framework Migrations Error");
                    }
                }
            }

            return app;
        }
    }
}
