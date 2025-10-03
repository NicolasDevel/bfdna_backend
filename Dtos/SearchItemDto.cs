namespace PokeApiProxy.Dtos;

public class SearchItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string ImageUrl { get; set; } = "";
    public List<string> Types { get; set; } = new();
    public List<string> Abilities { get; set; } = new();
    public long Popularity { get; set; }
    public double Score { get; set; }
}
