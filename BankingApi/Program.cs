using BankingApi.Interfaces.Repositories;
using BankingApi.Repositories;
using BankingApi.Interfaces.Services;
using BankingApi.Services;
using BankingApi.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler =
   System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;

});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();




// Configurar DbContext con SQLite
builder.Services.AddDbContext<BankingDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registrar Repositorios para Inyección de Dependencias
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

// Registrar Servicios para Inyección de Dependencias
builder.Services.AddScoped<IBankingService, BankingService>();


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
