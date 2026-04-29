using markit.Infrastructure;
using markit.Application;
using System.Text.Json.Serialization;
using markit.API.Middleware;
using markit.Infrastructure.Persistence.EF;
using Serilog;
using System.Text.Json;
using Hangfire;
using markit.Application.Helpers;
using Microsoft.AspNetCore.HttpOverrides;
using markit.Application.Models.Settings;

var builder = WebApplication.CreateBuilder(args);

// -- Add services to the container. --

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add support to logging with SERILOG
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    });

builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddMemoryCache();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();

// -- Add Cors Policy. --
builder.Services.AddCors(options =>
{
    SpaSettings spaSettings = new();
    builder.Configuration.GetSection(GeneralConstant.Configuration.SPA_SECTION_NAME)
        .Bind(spaSettings);

    options.AddPolicy("CorsPolicy", builder =>
        builder.WithOrigins(spaSettings.BaseUrl)
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials()
    );
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor
    | ForwardedHeaders.XForwardedHost
    | ForwardedHeaders.XForwardedProto
    | ForwardedHeaders.XForwardedPrefix
});

app.UseCors("CorsPolicy");
app.UseSerilogRequestLogging();
app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.UseHangfireDashboard();
app.MapControllers();

await app.SeedDatabase(builder.Configuration);
app.Run();
