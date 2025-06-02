using EasySaveProject.Core.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(); // Ajouter le support des contrôleurs

// Configuration OpenAPI/Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo 
    { 
        Title = "EasySave API", 
        Version = "v1",
        Description = "API pour la gestion des sauvegardes EasySave"
    });
});

// Ajouter CORS si nécessaire
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "EasySave API V1");
        c.RoutePrefix = "swagger"; // Accessible à /swagger
    });
}

app.UseHttpsRedirection();
app.UseCors(); // Utiliser CORS si configuré

// Mapper les contrôleurs au lieu des endpoints manuels
app.MapControllers();

app.Run();