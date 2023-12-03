using CustomBackend.Api.Infra;
using CustomBackend.Domain.Common.Services;
using CustomBackend.Infra.Notifications;
using CustomBackend.Infra.Tokens;
using CustomBackend.Infra.Utils;
using CustomBackend.Repository.Db.Context;
using CustomBackend.Repository.Db.Interceptors;
using CustomBackend.Repository.Db.Repositories.Common;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
var settings = builder.SettingSetup();
var assembly = Assembly.GetExecutingAssembly();

// Add services to the container.
builder.Services
    .AddSingleton(settings)
    .AddSingleton<UpdateAuditableInterceptor>()
    .AddScoped<NotificationManager>()
    .AddScoped<JwtTokenManager>()
    .AddCustomControllers()
    .AddCustomApiVersioning()
    .AddCustomAuthentication(settings)
    .AddCustomSwaggerDocGenForApiVersioning(assembly.GetName().Name, assembly.GetNameDateVersionatedName())

    //Utilizando o InMemoryDatabase para testes!
    .AddDbContext<MainDbContext>(opt => opt.UseInMemoryDatabase("CustomDb"))
    // .AddSqlServerDb(settings)

    .AddHttpContextAccessor()
    .AddResponseCaching()
    .AddScopedByBaseType<ServiceBase>()
    .AddScopedByBaseType<RepositoryDbBase>()
    ;


var app = builder.Build();

// Configure the HTTP request pipeline.
app
    .UseCustomCors()
    .UseHttpsRedirection()
    .UseResponseCaching()
    .UseCustomSwagger(app.Services.GetService<IApiVersionDescriptionProvider>())

    //Utilizando o InMemoryDatabase para testes!
    // .UseDatabaseInitialization(app.Services.GetService<ILogger<WebApplication>>(), settings)

    .UseAuthentication()
    .UseAuthorization()
    ;

app.UseControllersEndpoints();

app.Run();