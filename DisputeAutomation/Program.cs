using BusinessLogicLayer.Manager;
using BusinessLogicLayer.Manager.BREB;
using BusinessLogicLayer.ServiceInvoker;
using DataAccessLayer.Repositories;
using DataAccessLayer.Repositories.BREB;
using DataAccessLayer.ServiceInvoker;
using DisputeAutomation.DAL.Database;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ===== BREB Client Services =====
builder.Services.AddScoped<IBrebManager, BrebRepository>();
builder.Services.AddHttpClient<BrebService>();
builder.Services.AddScoped<IClientManager, ClientRepo>();
builder.Services.AddScoped<IBrebService, BrebService>();

// ===== DESCO Client Services =====
// TODO: Add DESCO-specific services when implemented


// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Dispute Automation API",
        Version = "v1",
        Description = "Multi-Client Dispute Automation System (BREB, WASA, DESCO, DPDC)"
    });
});

// Database connections
builder.Services.AddDbContext<DBConfiguration>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MTBBillCollectionDB")));

// Add logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
    logging.SetMinimumLevel(LogLevel.Information);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dispute Automation API v1");
        c.RoutePrefix = "swagger";
    });

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