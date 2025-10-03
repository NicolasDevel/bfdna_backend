using Microsoft.Extensions.Options;
using PokeApiProxy.Domain.Entities;
using PokeApiProxy.Repositories;
using PokeApiProxy.Repository;
using PokeApiProxy.Services;
namespace PokeApiProxy.Endpoints;

public static class PokemonEndpoints
{
    public static void MapPokemonEndpoints(this WebApplication app)
    {

        app.MapPost("/cache/pokemons", async (PokemonCacheService cacheService) =>
        {
            var count = await cacheService.BuildInitialCacheAsync();
            return Results.Ok($"{count} pokemons cacheados en DynamoDB");
        });

        app.MapGet("/pokemons", async (
            PokemonService queryService,
            string? type,
            string? ability,
            int page = 1,
            int pageSize = 20) =>
        {
            var result = await queryService.GetPokemonsAsync(type, ability, page, pageSize);
            return Results.Ok(result);
        });

        app.MapGet("/pokemons/{id:int}", async (PokemonService service, int id) =>
        {
            var pokemon = await service.GetPokemonByIdAsync(id);
            return pokemon is not null ? Results.Ok(pokemon) : Results.NotFound();
        });
    }
}