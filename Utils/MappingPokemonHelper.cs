using PokeApiProxy.Domain.Entities;
using System.Text.Json.Nodes;

namespace PokeApiProxy.Utils;

public static class MappingPokemonHelper
{
    public static Pokemon MapToDomain(JsonObject rawPokemon)
    {
        int id = (int)rawPokemon["id"]!;
        string name = (string)rawPokemon["name"]!;

        var types = rawPokemon["types"]!
            .AsArray()
            .Select(t => (string)t["type"]["name"])
            .ToList();

        var abilities = rawPokemon["abilities"]!
            .AsArray()
            .Select(a => (string)a["ability"]["name"])
            .ToList();

        string imageUrl = (string)rawPokemon["sprites"]!["other"]!["official-artwork"]!["front_default"]!;

        return new Pokemon
        {
            Id = id,
            Name = name,
            Types = types,
            Abilities = abilities,
            ImageUrl = imageUrl,
            LastUpdated = DateTime.UtcNow
        };

    }
}
