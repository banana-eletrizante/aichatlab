# AI Chat Lab

App Windows que gera e publica sites de chat de IA.

Tema + nome viram um chat pronto na Vercel. Cada visitante cola a **própria** chave OpenRouter. O token da Vercel fica criptografado com DPAPI neste computador.

- Site: [andre-rosler.vercel.app/aichatlab](https://andre-rosler.vercel.app/aichatlab)
- Autor: André Rösler
- Versão atual: **v1.3.0**

## 1.3.0

- Resposta em streaming (SSE) no chat publicado
- Duplo clique no histórico reabre o experimento
- Deploy READY abre a URL
- Chave/modelo/conversa salvos por slug
- Copiar resposta e limite de tamanho da mensagem

## Abrir

```bash
dotnet restore
dotnet run --project AIChatLab/AIChatLab.csproj
```

Baixe o `.exe` nas [releases](https://github.com/banana-eletrizante/aichatlab/releases).

## Licença

MIT © André Rösler
