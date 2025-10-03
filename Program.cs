using Amazon.DynamoDBv2;
using PokeApiProxy.Endpoints;
using PokeApiProxy.Repositories;
using PokeApiProxy.Repository;
using PokeApiProxy.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAWSLambdaHosting(LambdaEventSource.RestApi);

//AWS
builder.Services.AddAWSService<IAmazonDynamoDB>();

//Libraries
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();

//Services
builder.Services.AddScoped<PokemonCacheService>();
builder.Services.AddScoped<PokemonService>();

//Repositories
builder.Services.AddScoped<DynamoPokemonRepository>();
builder.Services.AddScoped<PokemonApiRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//endpoints
app.MapPokemonEndpoints();

app.Run();
