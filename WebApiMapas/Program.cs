var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseSwaggerUI(opitions =>
{
    opitions.RoutePrefix = "documentação";
    opitions.SwaggerEndpoint("/swagger/v1/swagger.json", "API de Geolocalização v1");

});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
