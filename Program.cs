using Microsoft.EntityFrameworkCore;
using RedisIntegration.Data;

var builder = WebApplication.CreateBuilder(args);


ConfigurationManager configuration = builder.Configuration;

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


//enable in-memory cach
builder.Services.AddMemoryCache();

//injecting db context
builder.Services.AddDbContext<OmsContext>(x => x.UseSqlServer("Server=.;Database=OMS;Trusted_Connection=True;TrustServerCertificate=True"));

//enable distributed cache
builder.Services.AddStackExchangeRedisCache(options => { options.Configuration = configuration["RedisCacheUrl"]; });


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
