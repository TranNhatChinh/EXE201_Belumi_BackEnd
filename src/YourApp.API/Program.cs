using YourApp.Application;
using YourApp.Infrastructure.Extensions;
using YourApp.API.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddExceptionHandler<YourApp.API.Common.GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Register Layers
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseExceptionHandler();

// Map Minimal API Endpoints
app.MapAuthEndpoints();

app.Run();
