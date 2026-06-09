# ADR-004 — Dashboard do Aspire (sem export externo)

**Status:** Aceito · **Data:** 2026-06-09

## Contexto

A telemetria (traces/métricas/logs) precisa ser observada. O Aspire traz um **dashboard
embutido** (receptor OTLP + UI). Alternativamente, poderíamos exportar para um stack externo
(Tempo/Prometheus/Loki, como no lab `observability-from-scratch`).

## Decisão

Usar **apenas o dashboard do Aspire** como backend de observabilidade do lab. O foco é mostrar
o que o Aspire entrega de graça para o ciclo de desenvolvimento.

## Consequências

- ✅ Zero configuração de backend — o trace distribuído aparece no dashboard imediatamente.
- ✅ Mantém o lab enxuto e focado no Aspire.
- ⚠️ O dashboard é para **desenvolvimento**, não produção (dados em memória, efêmeros).
- ➡️ Em produção, exportar OTLP para backends gerenciados — caminho mostrado no lab
  `observability-from-scratch`. Os ServiceDefaults já fazem isso com `OTEL_EXPORTER_OTLP_ENDPOINT`.
