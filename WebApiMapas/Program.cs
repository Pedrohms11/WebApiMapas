using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using WebApiMapas.Data;
using WebApiMapas.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<FirestoreService>();
builder.Services.AddScoped<LocalizacaoService>();

var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI(options =>
{
    // Define a rota raiz para acessar a documentação Swagger
    options.RoutePrefix = string.Empty;
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "API de Geolocalização v1");
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();