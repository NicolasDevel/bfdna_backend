# Algoritmo de sugerencia


## 🚀 Endpoints

* **GET** `/api/suggestions?q=&limit=&type=&ability=`
  Endpoint de sugerencias con ranking por prefijo, substring, fuzzy matching, popularidad y coincidencia de filtros.



## 📊 Algoritmo de sugerencias

El servicio `SuggestionService` implementa un motor de búsqueda con **ranking ponderado**.

### Criterios de ranking

* **Prefijo**: peso `0.40`
* **Substring**: peso `0.15`
* **Fuzzy (Levenshtein + trigramas)**: peso `0.10`
* **Popularidad**: peso `0.25`
* **Coincidencia de tipo**: `0.06`
* **Coincidencia de habilidad**: `0.04`

### Score

```text
score = 0.40*prefix + 0.15*substring + 0.10*fuzzy +
        0.25*popularity + 0.06*typeMatch + 0.04*abilityMatch
```

### Flujo

1. Busca candidatos que:

   * Empiecen con la query,
   * Contengan la query,
   * Coincidan en trigramas.
     Si no hay candidatos → se devuelven los 50 más populares.
2. Se calcula el score de cada candidato.
3. Se devuelven los `top N` ordenados por score → popularidad → nombre.
4. El resultado se cachea en memoria por 60 segundos.

### Pseudocódigo

```pseudo
function getSuggestions(q, type, ability, top=5):
    if cache has key(q,type,ability,top):
        return cache[key]

    candidates = []
    for item in items:
        if startsWith(item.name, q) or contains(item.name, q) or trigramMatch(item,q):
            candidates.add(item)

    if candidates.empty:
        candidates = topByPopularity(items, 50)

    scored = []
    for c in candidates:
        score = 0.40*prefixMatch(c,q) +
                0.15*substringMatch(c,q) +
                0.10*fuzzyMatch(c,q) +
                0.25*popularityScore(c) +
                0.06*typeMatch(c,type) +
                0.04*abilityMatch(c,ability)
        scored.add((c,score))

    return topN(scored.sortBy(score,popularity,name), top)
```

### Complejidad

* **Preprocesamiento (Initialize)**: `O(N * L)`
* **Búsqueda**: `O(N)` para filtrar + `O(C * L)` para score.
  Con `N = 10k–50k` pokémon, se mantiene eficiente en memoria.

### Ejemplo

Consulta: `"pika"`

* Prefijo: Pikachu = 1.0
* Substring: Pikachu = 1.0
* Fuzzy: alta similitud
* Popularidad: alta
  → **Pikachu rankea primero**.

---

## 📝 ADRs

* **DynamoDB** elegido para cachear Pokémon: escalable, serverless, y bajo costo.
* **.NET 8 minimal API** por rapidez de arranque y claridad.
* **Algoritmo híbrido (prefijo + substring + trigramas + Levenshtein)** para balance entre precisión y eficiencia.
