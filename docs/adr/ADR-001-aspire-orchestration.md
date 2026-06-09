# ADR-001 — .NET Aspire como orquestrador (AppHost + ServiceDefaults)

**Status:** Aceito · **Data:** 2026-06-09

## Contexto

Uma app distribuída (vários serviços + Postgres + Redis) precisa de orquestração local,
service discovery, injeção de connection strings, telemetria e um lugar para observá-la.
A alternativa é `docker-compose` + fiação manual de cada um desses aspectos — o caminho que
o lab `observability-from-scratch` percorre **de propósito**, para mostrar a mecânica.

## Decisão

Usar **.NET Aspire**: o **AppHost** declara recursos e serviços em C# e os orquestra; o
**ServiceDefaults** padroniza telemetria, health, discovery e resiliência em cada serviço.

## Consequências

- ✅ Orquestração, discovery e wiring de connection strings viram código declarativo, não YAML.
- ✅ Dashboard, health e OpenTelemetry "de graça" — o ponto deste lab.
- ✅ Contraponto didático direto ao lab feito na mão.
- ⚠️ Aspire usa Docker por baixo (Postgres/Redis como containers) e o DCP para orquestrar.
- ⚠️ Acoplamento ao modelo do Aspire; mitigado por ele ser open-source e baseado em NuGet.
