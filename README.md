# AI Chat Lab

App Windows que gera e publica sites de chat de IA.

Tema + nome viram um chat pronto na Vercel. Cada visitante cola a **própria** chave OpenRouter. O token da Vercel é do operador e fica criptografado com DPAPI — nunca entra no pacote gerado.

- Site: [andre-rosler.vercel.app/aichatlab](https://andre-rosler.vercel.app/aichatlab)
- Autor: André Rösler
- Identidade: Erlenmeyer lab-green (`#06140f` / `#22c55e`)
- Versão atual: **v1.1**

## Download

- App Windows (self-contained): https://github.com/banana-eletrizante/aichatlab/releases/download/v1.1/AIChatLab_v1.1.exe
- Página: https://andre-rosler.vercel.app/aichatlab
- Código: `git clone https://github.com/banana-eletrizante/aichatlab.git`

```bash
dotnet publish AIChatLab/AIChatLab.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

## Requisitos

- Windows 10/11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022 / Rider / `dotnet` CLI
- WebView2 Runtime (já vem no Windows 11)

## Abrir

```bash
dotnet restore
dotnet run --project AIChatLab/AIChatLab.csproj
```

## Como funciona

1. Defina o experimento (nome, tema, persona).
2. O app monta `index.html`, `styles.css`, `app.js`, `api/chat.js`, `vercel.json`.
3. Preview local (echo) no WebView2 — sem chave.
4. O visitante usa a própria chave OpenRouter no browser.
5. Publique com o token Vercel do operador.

A persona fica visível no HTML gerado — trate-a como pública.

## Segurança

- Zero chave OpenRouter no app, nos templates e no deploy.
- Token Vercel: DPAPI `CurrentUser`.
- Function Vercel só faz proxy com o header `x-openrouter-key`.

## Licença

MIT © André Rösler
