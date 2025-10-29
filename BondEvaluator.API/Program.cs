using BondEvaluator.API.Middlewares;
using BondEvaluator.Application.Configuration;
using BondEvaluator.Application.Helpers;
using BondEvaluator.Application.Helpers.Interface;
using BondEvaluator.Application.Services;
using BondEvaluator.Infrastructure.DependencyInjection;
using Serilog;

namespace BondEvaluator.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddRouting(options=>options.LowercaseUrls = true);
        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        // Configure logger
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Infinite)
            .CreateLogger();
        builder.Host.UseSerilog();

        builder.Services.AddScoped<IBondEvaluatorService, BondEvaluatorService>();
        builder.Services.AddSingleton<IRateParser, RateParser>();
        builder.Services.RegisterExternalServices();

        builder.Services.Configure<BondEvaluatorConfiguration>(
            builder.Configuration.GetSection(BondEvaluatorConfiguration.Section));

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }
        app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Swagger"));
        app.UseHttpsRedirection();

        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}