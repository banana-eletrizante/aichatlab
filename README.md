# AI Chat Lab

App Windows que gera e publica sites de chat de IA.

Tema + nome viram um chat pronto na Vercel. Cada visitante cola a **própria** chave OpenRouter. O token da Vercel é do operador e fica criptografado com DPAPI.

- Site: [andre-rosler.vercel.app/aichatlab](https://andre-rosler.vercel.app/aichatlab)
- Autor: André Rösler
- Versão atual: **v1.2.1**

## 1.2.1

- Duplo clique no histórico reabre o experimento
- Deploy bem-sucedido abre a URL no navegador
- Chat gerado guarda chave/modelo por slug e copia resposta
- Limite de tamanho da mensagem no cliente e na function

## Abrir

```bash
dotnet restore
dotnet run --project AIChatLab/AIChatLab.csproj
```

## Licença

MIT © André Rösler
