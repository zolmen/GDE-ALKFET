using System.Text.Json;
using CertStore.API.Services;
using CertStore.API.Services.Interfaces;
using CertStore.Infrastructure;
using CertStore.Infrastructure.UnitOfWork;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAnyOrigin", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

var mongoContext = new MongoDbContext(builder.Configuration);
builder.Services.AddSingleton(mongoContext.Database);

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IRootCertService, RootCertService>();
builder.Services.AddScoped<IUserCertService, UserCertService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAnyOrigin");
app.MapControllers();
app.MapMcp("/mcp");
   
await app.RunAsync();
