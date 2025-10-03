using System.Net.Http.Json;
using System.Text.Json.Nodes;
using PokeApiProxy.Dtos;

namespace PokeApiProxy.Repositories;

public class PokemonApiRepository
{
    private readonly HttpClient _httpClient;

    public PokemonApiRepository(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<JsonObject?> GetPokemonByIdAsync(int id)
    {
        string url = $"https://pokeapi.co/api/v2/pokemon/{id}";
        return await _httpClient.GetFromJsonAsync<JsonObject>(url);
    }

    public async Task<PokemonListDto?> GetPokemonsAsync(int offset, int limit)
    {
        string url = $"https://pokeapi.co/api/v2/pokemon?limit={limit}&offset={offset}";
        return await _httpClient.GetFromJsonAsync<PokemonListDto>(url);
    }

    public async Task<JsonObject?> GetPokemonDetailFromUrlAsync(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("La URL del Pokémon no puede estar vacía.");
        try
        {
            return await _httpClient.GetFromJsonAsync<JsonObject>(url);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Error al obtener datos desde {url}: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error inesperado al obtener datos desde {url}: {ex.Message}");
            return null;
        }
    }
}