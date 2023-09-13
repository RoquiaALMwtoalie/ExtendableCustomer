using ExtendableCustomerApi.IServices;
using ExtendableCustomerApi.IServices.ICompanyServices;
using ExtendableCustomerApi.IServices.IContactServices;
using ExtendableCustomerApi.Model;
using ExtendableCustomerApi.Services;
using ExtendableCustomerApi.Services.ContactServices;
using MongoDB.Driver;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<ExtendableCustomerDatabaseSettings>(
    builder.Configuration.GetSection("ExtendableCustomerDatabase"));



// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IMongoClient, MongoClient>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IContactService, ContactService>();
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
