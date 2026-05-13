using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using WebApiMapas.Data;
using WebApiMapas.Service;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Registro de clientes HTTP necessário para a validação de mapas
builder.Services.AddHttpClient();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "API de Geolocalização",
        Version = "v1",
        Description = "API para gestão de mapas e localização integrada ao Firestore."
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

// Configuração do Firestore e Injeção de Dependência
builder.Services.AddSingleton<FirestoreService>();
builder.Services.AddScoped<LocalizacaoService>();

// CONFIGURAÇÃO DAS CREDENCIAIS DO FIREBASE
var caminhoCredenciais = Path.Combine(Directory.GetCurrentDirectory(),
    "config_API/firebase-key.json");
var credential = GoogleCredential.FromFile(caminhoCredenciais);

var firestoreDb = new FirestoreDbBuilder
{
    ProjectId = "web-api-mapas",
    Credential = credential
}.Build();

// Configuração de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTudo",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.RoutePrefix = string.Empty;
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "API de Geolocalização v1");
});

app.UseCors("PermitirTudo");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();