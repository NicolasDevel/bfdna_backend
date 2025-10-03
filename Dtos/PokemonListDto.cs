namespace PokeApiProxy.Dtos;

public class PokemonListDto
{
    public int Count { get; set; }
    public string? Next { get; set; }
    public string? Previous { get; set; }
    public List<PokemonItemDto> Results { get; set; } = new();
}

public class PokemonItemDto
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
