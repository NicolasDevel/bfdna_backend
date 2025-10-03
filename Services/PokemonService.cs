using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PokeApiProxy.Domain.Entities;
using PokeApiProxy.Dtos;
using PokeApiProxy.Repository;

namespace PokeApiProxy.Services;

public class PokemonService
{
    private readonly DynamoPokemonRepository _dynamoRepo;

    public PokemonService(DynamoPokemonRepository dynamoRepo)
    {
        _dynamoRepo = dynamoRepo;
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
}
