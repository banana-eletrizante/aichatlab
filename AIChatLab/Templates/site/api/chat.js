export default async function handler(req, res) {
  if (req.method === "OPTIONS") {
    res.setHeader("Access-Control-Allow-Origin", req.headers.origin || "");
    res.setHeader("Access-Control-Allow-Headers", "content-type, x-openrouter-key");
    res.setHeader("Access-Control-Allow-Methods", "POST, OPTIONS");
    res.status(204).end();
    return;
  }
  if (req.method !== "POST") {
    res.status(405).json({ error: "method_not_allowed" });
    return;
  }
  const rawLen = Number(req.headers["content-length"] || 0);
  if (rawLen > 200000) {
    res.status(413).json({ error: "payload_too_large" });
    return;
  }
  const key = req.headers["x-openrouter-key"];
  if (!key || !String(key).startsWith("sk-or-") || String(key).length < 20) {
    res.status(401).json({ error: "missing_or_key" });
    return;
  }
  const body = req.body || {};
  const { messages, model, temperature, max_tokens } = body;
  if (!Array.isArray(messages) || messages.length === 0 || messages.length > 80) {
    res.status(400).json({ error: "invalid_messages" });
    return;
  }
  if (typeof model !== "string" || model.length < 3 || model.length > 120) {
    res.status(400).json({ error: "invalid_model" });
    return;
  }
  const r = await fetch("https://openrouter.ai/api/v1/chat/completions", {
    method: "POST",
    headers: {
      Authorization: "Bearer " + key,
      "Content-Type": "application/json",
      "HTTP-Referer": req.headers.referer || "",
      "X-Title": "{{NAME}}",
    },
    body: JSON.stringify({
      model,
      messages,
      temperature: typeof temperature === "number" ? temperature : 0.7,
      max_tokens: typeof max_tokens === "number" ? max_tokens : 1024,
      stream: false,
    }),
  });
  const text = await r.text();
  res.status(r.status);
  res.setHeader("Content-Type", "application/json");
  res.send(text);
}
