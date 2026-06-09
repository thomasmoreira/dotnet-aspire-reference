# ADR-005 — Verificação via Aspire.Hosting.Testing

**Status:** Aceito · **Data:** 2026-06-09

## Contexto

O lab precisa ser verificável ao vivo (a régua de qualidade do portfólio). Rodar o AppHost
manualmente (`dotnet run`) e usar `curl` funciona, mas o teardown do DCP é frágil e não é
automatizável de forma confiável.

## Decisão

Verificar com **`Aspire.Hosting.Testing`**: o teste sobe o AppHost real (containers inclusos)
via `DistributedApplicationTestingBuilder`, espera os recursos ficarem saudáveis, exercita os
endpoints por HTTP e dá **dispose limpo** ao final.

## Consequências

- ✅ Verificação ao vivo, real (containers de verdade), e reprodutível com `dotnet test`.
- ✅ Lifecycle do app distribuído gerenciado pelo framework — sem processos órfãos.
- ✅ O teste vira um **entregável** do lab, não só um passo manual.
- ⚠️ Os testes exigem Docker; por isso a verificação ao vivo roda localmente (o CI faz build).
