using BusinessLogicLayer.Manager;
using BusinessLogicLayer.Repositories;
using DataAccessLayer.Repositories;
using DisputeAutomation.DAL.Database;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Register your dependencies
builder.Services.AddScoped<IDBRepository, DBRepository>();
builder.Services.AddScoped<DisputeManager>();
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//db connection
builder.Services.AddDbContext<DBConfiguration>(options =>
       options.UseSqlServer(builder.Configuration.GetConnectionString("MTBBillCollectionDB")));

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.Use(async (context, next) =>
    {
        if (context.Request.Path == "/")
        {
            context.Response.Redirect("/swagger");
            return;
        }
        await next();
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
