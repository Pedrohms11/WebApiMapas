using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
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


// 1. Pegue o caminho físico do arquivo (ajuste o nome do arquivo json)
var caminhoCredenciais = Path.Combine(Directory.GetCurrentDirectory(), "config_API/firebase-key.json");

// 2. Crie a credencial lendo o arquivo
var credential = GoogleCredential.FromFile(caminhoCredenciais);

// 3. Inicialize o Firestore passando a credencial explicitamente
var firestoreDb = new FirestoreDbBuilder
{
    ProjectId = "web-api-mapas",
    Credential = credential
}.Build();

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