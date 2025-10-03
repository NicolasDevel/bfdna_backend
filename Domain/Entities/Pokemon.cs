namespace PokeApiProxy.Domain.Entities;

public class Pokemon
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<string> Types { get; set; } = new();
    public List<string> Abilities { get; set; } = new();
    public string ImageUrl { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public long Popularity { get; set; } = 0;
}