using Microsoft.Extensions.Caching.Memory;
using PokeApiProxy.Domain.Entities;
using PokeApiProxy.Dtos;
using PokeApiProxy.Repository;

namespace PokeApiProxy.Services;

public class SuggestionService
{
    private readonly DynamoPokemonRepository _dynamoRepo;
    private readonly IMemoryCache _memoryCache;
    private readonly object _initLock = new();
    private List<SearchItem> _items = new();
    private int _maxPopularity = 1;
    private bool _initialized = false;

    private readonly double prefixWeight = 0.40;
    private readonly double substringWeight = 0.15;
    private readonly double fuzzyWeight = 0.10;
    private readonly double popularityWeight = 0.25;
    private readonly double typeWeight = 0.06;
    private readonly double abilityWeight = 0.04;

    public SuggestionService(DynamoPokemonRepository dynamoRepo, IMemoryCache memoryCache)
    {
        _dynamoRepo = dynamoRepo;
        _memoryCache = memoryCache;
    }

    public async Task InitializeIfNeededAsync()
    {
        if (_initialized) return;

        lock (_initLock)
        {
            if (_initialized) return;
        }

        var pokemons = await _dynamoRepo.GetAllPokemonsAsync();
        
        var items = pokemons.Select(p => new SearchItem
        {
            Id = p.Id,
            Name = p.Name,
            NameLower = p.Name.ToLowerInvariant(),
            TypesLower = p.Types.Select(t => t.ToLowerInvariant()).ToList(),
            AbilitiesLower = p.Abilities.Select(a => a.ToLowerInvariant()).ToList(),
            ImageUrl = p.ImageUrl,
            Popularity = p.Popularity,
            Trigrams = BuildTrigrams(p.Name.ToLowerInvariant())
        }).ToList();

        _items = items;
        _maxPopularity = (int)Math.Max(1, items.Max(i => i.Popularity));
        _initialized = true;
    }

    public async Task<List<SearchItemDto>> GetSuggestionsAsync(string q, int top = 5, string? type = null, string? ability = null)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
            return new List<SearchItemDto>();

        await InitializeIfNeededAsync();

        var qLower = q.Trim().ToLowerInvariant();
        var cacheKey = $"sugs:{qLower}:{type ?? ""}:{ability ?? ""}:{top}";

        if (_memoryCache.TryGetValue(cacheKey, out List<SearchItemDto> cached))
            return cached;

        var qTrigrams = BuildTrigrams(qLower);

        var candidates = _items.Where(item =>
            item.NameLower.StartsWith(qLower) ||
            item.NameLower.Contains(qLower) ||
            TrigramIntersectionCount(item.Trigrams, qTrigrams) > 0
        ).ToList();

        if (!candidates.Any())
            candidates = _items.OrderByDescending(x => x.Popularity).Take(50).ToList();

        var scored = candidates.Select(c =>
        {
            double prefixScore = c.NameLower.StartsWith(qLower) ? 1.0 : 0.0;
            double substringScore = c.NameLower.Contains(qLower) ? 1.0 : 0.0;
            double fuzzyScore = ComputeFuzzyNormalized(qLower, c.NameLower);
            double popNorm = (double)c.Popularity / Math.Max(1, _maxPopularity);
            bool typeMatch = !string.IsNullOrWhiteSpace(type) && c.TypesLower.Contains(type.ToLowerInvariant());
            bool abilityMatch = !string.IsNullOrWhiteSpace(ability) && c.AbilitiesLower.Contains(ability.ToLowerInvariant());

            double score =
                prefixWeight * prefixScore +
                substringWeight * substringScore +
                fuzzyWeight * fuzzyScore +
                popularityWeight * popNorm +
                typeWeight * (typeMatch ? 1.0 : 0.0) +
                abilityWeight * (abilityMatch ? 1.0 : 0.0);

            return (c, score);
        });

        var results = scored
            .OrderByDescending(x => x.score)
            .ThenByDescending(x => x.c.Popularity)
            .ThenBy(x => x.c.Name)
            .Take(top)
            .Select(x => new SearchItemDto
            {
                Id = x.c.Id,
                Name = x.c.Name,
                ImageUrl = x.c.ImageUrl,
                Types = x.c.TypesLower,
                Abilities = x.c.AbilitiesLower,
                Score = Math.Round(x.score, 4),
                Popularity = x.c.Popularity
            }).ToList();

        _memoryCache.Set(cacheKey, results, TimeSpan.FromSeconds(60));

        return results;
    }

    private static HashSet<string> BuildTrigrams(string s)
    {
        var t = new HashSet<string>();
        var ss = s.Replace(" ", "_");
        for (int i = 0; i + 3 <= ss.Length; i++)
            t.Add(ss.Substring(i, 3));
        return t;
    }

    private static int TrigramIntersectionCount(HashSet<string> a, HashSet<string> b)
    {
        if (a == null || b == null) return 0;
        return a.Count(b.Contains);
    }

    private static double ComputeFuzzyNormalized(string q, string name)
    {
        int maxLen = Math.Max(q.Length, name.Length);
        int dist = LevenshteinDistance(q, name, Math.Min(3, Math.Max(1, q.Length / 2)));
        if (dist < 0) return 0.0;
        return Math.Max(0.0, 1.0 - ((double)dist / maxLen));
    }

    private static int LevenshteinDistance(string s, string t, int maxAllowed)
    {
        if (s == t) return 0;
        if (Math.Abs(s.Length - t.Length) > maxAllowed) return -1;
        int n = s.Length;
        int m = t.Length;
        var prev = new int[m + 1];
        var cur = new int[m + 1];
        for (int j = 0; j <= m; j++) prev[j] = j;

        for (int i = 1; i <= n; i++)
        {
            cur[0] = i;
            int minRow = cur[0];
            for (int j = 1; j <= m; j++)
            {
                int cost = s[i - 1] == t[j - 1] ? 0 : 1;
                cur[j] = Math.Min(Math.Min(prev[j] + 1, cur[j - 1] + 1), prev[j - 1] + cost);
                minRow = Math.Min(minRow, cur[j]);
            }
            if (minRow > maxAllowed) return -1;
            var tmp = prev; prev = cur; cur = tmp;
        }
        return prev[m] <= maxAllowed ? prev[m] : -1;
    }

    private class SearchItem
    {
        public int Id;
        public string Name = "";
        public string NameLower = "";
        public List<string> TypesLower = new();
        public List<string> AbilitiesLower = new();
        public string ImageUrl = "";
        public long Popularity = 0;
        public HashSet<string> Trigrams = new();
    }
}
