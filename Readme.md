# PokeApiProxy - Backend

Este proyecto implementa un **backend serverless en .NET 8 Minimal API** desplegado en AWS Lambda con API Gateway.
Integra con la [PokéAPI](https://pokeapi.co/) y utiliza **DynamoDB** como caché persistente para pokemones.

## Recursos
* **Url del backend `https://7tnrtr29ok.execute-api.us-east-1.amazonaws.com/dev/`**


## 🚀 Endpoints

* **GET `/api/pokemon?search=&type=&ability=&page=`**
  Lista pokemones desde DynamoDB, con filtros por nombre, tipo o habilidad y paginación.

* **GET `/api/pokemon/{idOrName}`**
  Obtiene el detalle de un pokemon.

  * Si existe en DynamoDB → se devuelve y se refresca con PokéAPI.
  * Si no existe en DynamoDB → se obtiene de PokéAPI y se guarda en DynamoDB.
  * Incrementa el campo **Popularity**.

* **GET `/api/suggestions?q=&limit=&type=&ability=`**
  Retorna sugerencias inteligentes de pokemones usando:

  * Prefijo, substring y fuzzy (trigramas + Levenshtein).
  * Popularidad.
  * Coincidencia por tipo/habilidad.

* **GET `/swagger`**
  Documentación OpenAPI interactiva (Swagger UI).

---

## 🏗️ Arquitectura

* **AWS Lambda + API Gateway**: despliegue sin servidores.
* **DynamoDB**: almacenamiento de pokemones cacheados.
* **PokéAPI**: fuente de datos externa.
* **CloudWatch Logs**: logs de observabilidad.
* **Serverless Framework**: IaC y despliegue automático.

📌 Diagrama de arquitectura:


![Arquitectura Backend](./docs/Diagrama_Backend.svg)


---

## ⚙️ Requisitos

* [.NET 8 SDK](https://dotnet.microsoft.com/download)
* [Serverless Framework](https://www.serverless.com/) (`npm install -g serverless`)
* [AWS CLI](https://aws.amazon.com/cli/) configurado
* Cuenta AWS con permisos en:

  * Lambda
  * API Gateway
  * DynamoDB
  * S3 (para empaquetado)

---

## 📦 Build y Deploy

### Local

```bash
dotnet restore
dotnet build
dotnet run
```

La API quedará disponible en `http://localhost:5156/swagger`.

### AWS

```bash
sls deploy
```

Esto:

1. Compila y publica en `bin/Release/net8.0/publish`.
2. Empaqueta el ZIP y lo sube a S3.
3. Crea Lambda + API Gateway + DynamoDB en `us-east-1`.

### Eliminar infraestructura

```bash
sls remove
```
---

## 📊 Observabilidad

* Logs en CloudWatch (request/response).
* Métricas básicas:

  * Latencia API.
  * Cache hit/miss en Dynamo.

---

## 💰 Costos estimados AWS

* **DynamoDB**: ~$1–2/mes con plan on-demand y 50k items.
* **Lambda**: <$1/mes con 1M invocaciones free tier.
* **API Gateway**: ~$3/mes (dependiendo del tráfico).
* **Total estimado**: $5–6/mes en entorno dev.

---

## 📄 ADR (Architectural Decision Record)

* **Lenguaje**: .NET 8 Minimal API → bonus y simplicidad.
* **Infraestructura**: Serverless Framework para IaC → rápido y reproducible.
* **Cache**: DynamoDB  → escalabilidad y persistencia.
* **Algoritmo de sugerencias**: trigramas + Levenshtein + popularidad.
* **Despliegue**: AWS Lambda por simplicidad y bajo costo.

---

## 👨‍💻 Autor

NicoDevel

---
