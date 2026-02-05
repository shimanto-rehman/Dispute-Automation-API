var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Application services & repositories
builder.Services.AddScoped<BusinessLogicLayer.Manager.ICollectionManager, BusinessLogicLayer.Manager.CollectionManager>();
builder.Services.AddScoped<BusinessLogicLayer.Processor.ICollectionProcessor, BusinessLogicLayer.Processor.CollectionProcessor>();
builder.Services.AddScoped<DataAccessLayer.Repositories.ICollectionRepository, DataAccessLayer.Repositories.SqlServer.SqlServerCollectionRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
