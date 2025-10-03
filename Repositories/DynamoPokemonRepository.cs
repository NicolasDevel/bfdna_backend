using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using PokeApiProxy.Domain.Entities;
using System.Text.Json;

namespace PokeApiProxy.Repository;

public class DynamoPokemonRepository
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private const string TableName = "PokemonsTable";

    public DynamoPokemonRepository(IAmazonDynamoDB dynamoDb)
    {
        _dynamoDb = dynamoDb;
    }

    public async Task SavePokemonAsync(Pokemon pokemon)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            ["Id"] = new AttributeValue { N = pokemon.Id.ToString() },
            ["Name"] = new AttributeValue { S = pokemon.Name },
            ["ImageUrl"] = new AttributeValue { S = string.IsNullOrWhiteSpace(pokemon.ImageUrl) ? "N/A" : pokemon.ImageUrl },
            ["LastUpdated"] = new AttributeValue { S = pokemon.LastUpdated.ToString("o") },
            ["Popularity"] = new AttributeValue { N = pokemon.Popularity.ToString() }
        };

        if (pokemon.Types != null && pokemon.Types.Any())
        {
            item["Types"] = new AttributeValue { SS = pokemon.Types.Distinct().ToList() };
        }

        if (pokemon.Abilities != null && pokemon.Abilities.Any())
        {
            item["Abilities"] = new AttributeValue { SS = pokemon.Abilities.Distinct().ToList() };
        }

        await _dynamoDb.PutItemAsync(new PutItemRequest
        {
            TableName = TableName,
            Item = item
        });
    }

    public async Task<List<Pokemon>> GetAllPokemonsAsync()
    {
        var result = await _dynamoDb.ScanAsync(new ScanRequest
        {
            TableName = TableName
        });

        return result.Items.Select(item => new Pokemon
        {
            Id = int.Parse(item["Id"].N),
            Name = item["Name"].S,
            Types = item.ContainsKey("Types") ? item["Types"].SS : new List<string>(),
            Abilities = item.ContainsKey("Abilities") ? item["Abilities"].SS : new List<string>(),
            ImageUrl = item["ImageUrl"].S,
            LastUpdated = DateTime.Parse(item["LastUpdated"].S),
            Popularity = long.Parse(item["Popularity"].N)
        }).OrderBy(p => p.Id).ToList();
    }
}
