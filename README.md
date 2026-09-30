# AI Chat Lab

App Windows que transforma nome, persona e tema num site de chat e publica na Vercel.

O visitante do site cola a **própria** chave OpenRouter. O token da Vercel fica só no seu PC, criptografado com DPAPI.

- Site: [andre-rosler.vercel.app/aichatlab](https://andre-rosler.vercel.app/aichatlab)
- Versão: **1.3**

## Fluxo

1. Nome, slug, tagline e persona
2. Cores / tema
3. Modelos OpenRouter permitidos
4. Preview local no WebView
5. Gerar pasta
6. Publicar na Vercel (abre a URL quando ficar READY)

O histórico guarda os experimentos. Duplo clique reabre o formulário.

## O site gerado

- Streaming SSE da OpenRouter
- Chave, modelo e conversa salvos por slug no `localStorage`
- Copiar resposta, parar geração, tema com grid e marca

## Abrir o app

```bash
dotnet restore
dotnet run --project AIChatLab/AIChatLab.csproj
```

`.exe` nas [releases](https://github.com/banana-eletrizante/aichatlab/releases). Para gerar: Actions → Release → `v1.3`.

MIT © André Rösler
