# {{NAME}}

{{TAGLINE}}

Chat gerado pelo [AI Chat Lab](https://andre-rosler.com/aichatlab).

## Publicar

1. Envie esta pasta para a Vercel (arrastar a pasta no dashboard ou `npx vercel`).
2. Cada visitante cola a **própria** chave OpenRouter no navegador.
3. A chave não vai para o autor do site. Este projeto não embute token da Vercel nem chave de modelo.

## API

`POST /api/chat` recebe `messages`, `model` e `stream`. A function só encaminha para a OpenRouter.
