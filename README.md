# AI Chat Lab

App Windows que gera e publica sites de chat de IA.

Tema + nome viram um chat pronto na Vercel. Cada visitante cola a **própria** chave OpenRouter. O token da Vercel é do operador e fica criptografado com DPAPI — nunca entra no pacote gerado.

Site: [andre-rosler.com/aichatlab](https://andre-rosler.com/aichatlab)  
Autor: André Rösler

## Requisitos

- Windows
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022 / Rider / `dotnet` CLI

## Abrir

```bash
dotnet restore
dotnet run --project AIChatLab/AIChatLab.csproj
```

Publicar exe (no Windows):

```bash
dotnet publish AIChatLab/AIChatLab.csproj -c Release -r win-x64 --self-contained false
```

## Como funciona

1. Defina o experimento (nome, tema, persona).
2. O app monta `index.html`, `styles.css`, `app.js`, `api/chat.js`, `vercel.json`.
3. O visitante usa a própria chave OpenRouter no browser.
4. Publique com o token Vercel do operador.

A persona fica visível no HTML gerado — trate-a como pública.

## Segurança

- Zero chave OpenRouter no app, nos templates e no deploy.
- Token Vercel: DPAPI `CurrentUser`.
- Function Vercel só faz proxy com o header `x-openrouter-key`.

## Licença

MIT © André Rösler
