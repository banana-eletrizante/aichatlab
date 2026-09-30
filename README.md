# AI Chat Lab

App Windows que gera e publica sites de chat de IA.

Tema + nome viram um chat pronto na Vercel. Cada visitante cola a **própria** chave OpenRouter. O token da Vercel é do operador e fica criptografado com DPAPI — nunca entra no pacote gerado.

- Site: [andre-rosler.vercel.app/aichatlab](https://andre-rosler.vercel.app/aichatlab)
- Autor: André Rösler
- Identidade: Erlenmeyer lab-green (`#06140f` / `#22c55e`)
- Versão atual: **v1.2**

## Novidades da 1.2

- Function `/api/chat` com limite de payload, tamanho de mensagem e try/catch de upstream
- Headers de segurança no `vercel.json`
- Lista de modelos atualizada (Gemini 2.5 Flash)

## Download

- App Windows (self-contained): veja a página de [Releases](https://github.com/banana-eletrizante/aichatlab/releases)
- Código: `git clone https://github.com/banana-eletrizante/aichatlab.git`

```bash
dotnet publish AIChatLab/AIChatLab.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

## Requisitos

- Windows 10/11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- WebView2 Runtime (já vem no Windows 11)

## Abrir

```bash
dotnet restore
dotnet run --project AIChatLab/AIChatLab.csproj
```

## Licença

MIT © André Rösler
