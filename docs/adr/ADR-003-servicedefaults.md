# ADR-003 — Service discovery + resiliência via ServiceDefaults

**Status:** Aceito · **Data:** 2026-06-09

## Contexto

Cada serviço precisa achar os outros e tolerar falhas transitórias. Isso pode ser feito com
URLs hardcoded + `HttpClient` cru, ou padronizado num único lugar.

## Decisão

Centralizar no projeto **ServiceDefaults** (`AddServiceDefaults()`), referenciado por todos os
serviços: **service discovery** (resolve `https+http://catalog` para o endereço real injetado
pelo AppHost) e **resiliência HTTP padrão** (retry, circuit breaker, timeout — Polly via
`AddStandardResilienceHandler`), além de OpenTelemetry e health checks.

## Consequências

- ✅ Zero URL hardcoded; endereços vêm do AppHost (`WithReference`).
- ✅ Resiliência por padrão em toda chamada serviço-a-serviço, sem repetição.
- ✅ Telemetria e health uniformes — um único ponto para evoluir a política.
- ⚠️ Comportamento "mágico" implícito; documentado aqui e nos comentários do código.
