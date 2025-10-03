using System.Text.Json;
using PokeApiProxy.Repositories;
using PokeApiProxy.Repository;
using PokeApiProxy.Utils;

namespace PokeApiProxy.Services;

public class PokemonCacheService
{
    private readonly PokemonApiRepository _repository;
    private readonly DynamoPokemonRepository _dynamoRepo;

    public PokemonCacheService(PokemonApiRepository repository, DynamoPokemonRepository dynamoRepo)
    {
        _repository = repository;
        _dynamoRepo = dynamoRepo;
    }

    public async Task<int> BuildInitialCacheAsync()
    {
        var list = await _repository.GetPokemonsAsync(0, 1309);
        if (list == null || !list.Results.Any())
            throw new Exception("No se pudo obtener la lista de Pokémon desde la PokéAPI");

        using var semaphore = new SemaphoreSlim(10);

        var tasks = list.Results.Select(async (item, index) =>
        {
            await semaphore.WaitAsync();
            try
            {
                var rawDetail = await _repository.GetPokemonDetailFromUrlAsync(item.Url);
                if (rawDetail != null)
                {
                    var pokemon = MappingPokemonHelper.MapToDomain(rawDetail);
                    await _dynamoRepo.SavePokemonAsync(pokemon);
                    return pokemon;
                }
                return null;
            }
            finally
            {
                semaphore.Release();
            }
        });

        var results = await Task.WhenAll(tasks);

        return results.Count(p => p != null);
    }
}

