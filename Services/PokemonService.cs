using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PokeApiProxy.Domain.Entities;
using PokeApiProxy.Dtos;
using PokeApiProxy.Repositories;
using PokeApiProxy.Repository;
using PokeApiProxy.Utils;

namespace PokeApiProxy.Services;

public class PokemonService
{
    private readonly DynamoPokemonRepository _dynamoRepo;
    private readonly PokemonApiRepository _pokemonRepo;

    public PokemonService(DynamoPokemonRepository dynamoRepo, PokemonApiRepository pokemonRepo)
    {
        _dynamoRepo = dynamoRepo;
        _pokemonRepo = pokemonRepo;
    }

    public async Task<PagedResult<Pokemon>> GetPokemonsAsync(
        string? type, string? ability,
        int page = 1, int pageSize = 20)
    {
        var pokemons = await _dynamoRepo.GetAllPokemonsAsync();

        if (!string.IsNullOrEmpty(type))
            pokemons = pokemons
                .Where(p => p.Types.Any(t => t.Equals(type, StringComparison.OrdinalIgnoreCase)))
                .ToList();

        if (!string.IsNullOrEmpty(ability))
            pokemons = pokemons
                .Where(p => p.Abilities.Any(a => a.Equals(ability, StringComparison.OrdinalIgnoreCase)))
                .ToList();

        var totalCount = pokemons.Count;

        var items = pokemons
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<Pokemon>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<Pokemon?> GetPokemonByIdAsync(int id)
    {
        var cached = await _dynamoRepo.GetPokemonByIdAsync(id);

        if (cached == null)
        {
            var detail = await _pokemonRepo.GetPokemonByIdAsync(id);
            if (detail == null) return null;

            var pokemon = MappingPokemonHelper.MapToDomain(detail);

            await _dynamoRepo.SavePokemonAsync(pokemon);

            return pokemon;
        }
        else
        {
            var detail = await _pokemonRepo.GetPokemonByIdAsync(id);
            if (detail != null)
            {
                var updated = MappingPokemonHelper.MapToDomain(detail);
                updated.Popularity = cached.Popularity + 1;
                await _dynamoRepo.SavePokemonAsync(updated);

                return updated;
            }

            cached.Popularity++;
            await _dynamoRepo.SavePokemonAsync(cached);
            return cached;
        }
    }
}
