using markit.Infraestructure;
using markit.Application;
using System.Text.Json.Serialization;
using markit.API.Middleware;
using markit.Infraestructure.Persistence.EF;

var builder = WebApplication.CreateBuilder(args);

// -- Add services to the container. --

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddInfraestructureServices(builder.Configuration);
builder.Services.AddApplicationServices();

// -- Add Cors Policy. --
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader()
    );
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("CorsPolicy");
app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.SeedDatabase(builder.Configuration);

app.Run();
