# ADR-002 — Comunicação HTTP síncrona entre os serviços

**Status:** Aceito · **Data:** 2026-06-09

## Contexto

O Gateway precisa compor dados de Catalog e Pricing. Isso pode ser síncrono (HTTP
request/response) ou assíncrono (mensageria). A escolha define o formato do trace distribuído
— o entregável central do lab.

## Decisão

**HTTP síncrono** entre os três serviços (Gateway → Catalog, Gateway → Pricing), via
`HttpClient` tipado com service discovery + resiliência dos ServiceDefaults.

## Consequências

- ✅ Trace distribuído **limpo e linear**, fácil de ler no dashboard — exatamente o killer detail.
- ✅ Mostra propagação de contexto (W3C `traceparent`) ponta a ponta sem plumbing manual.
- ✅ Mensageria assíncrona já é o tema do lab `distributed-consistency-lab` — sem sobreposição.
- ⚠️ Acoplamento temporal (o Gateway espera os downstreams); aceitável para o propósito do lab.
