export default async function handler(req, res) {
  const origin = req.headers.origin || "";
  res.setHeader("Access-Control-Allow-Origin", origin);
  res.setHeader("Access-Control-Allow-Headers", "content-type, x-openrouter-key");
  res.setHeader("Access-Control-Allow-Methods", "POST, OPTIONS");
  res.setHeader("Cache-Control", "no-store");

  if (req.method === "OPTIONS") {
    res.status(204).end();
    return;
  }
  if (req.method !== "POST") {
    res.status(405).json({ error: "method_not_allowed" });
    return;
  }
  const rawLen = Number(req.headers["content-length"] || 0);
  if (rawLen > 120000) {
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
  const wantStream = body.stream !== false;
  if (!Array.isArray(messages) || messages.length === 0 || messages.length > 40) {
    res.status(400).json({ error: "invalid_messages" });
    return;
  }
  const tooLong = messages.some((m) => typeof m?.content === "string" && m.content.length > 8000);
  if (tooLong) {
    res.status(400).json({ error: "message_too_long" });
    return;
  }
  if (typeof model !== "string" || model.length < 3 || model.length > 120) {
    res.status(400).json({ error: "invalid_model" });
    return;
  }
  const safeTemp = typeof temperature === "number" && temperature >= 0 && temperature <= 2 ? temperature : 0.7;
  const safeMax = typeof max_tokens === "number" && max_tokens > 0 && max_tokens <= 4096 ? max_tokens : 1024;
  try {
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
        temperature: safeTemp,
        max_tokens: safeMax,
        stream: wantStream,
      }),
    });

    if (!wantStream || !r.ok) {
      const text = await r.text();
      res.status(r.status);
      res.setHeader("Content-Type", "application/json");
      res.send(text);
      return;
    }

    res.status(r.status);
    res.setHeader("Content-Type", "text/event-stream; charset=utf-8");
    res.setHeader("Cache-Control", "no-cache");
    res.setHeader("Connection", "keep-alive");

    if (r.body && typeof r.body.getReader === "function") {
      const reader = r.body.getReader();
      const decoder = new TextDecoder();
      while (true) {
        const { done, value } = await reader.read();
        if (done) break;
        res.write(typeof value === "string" ? value : decoder.decode(value, { stream: true }));
      }
      res.end();
      return;
    }

    res.send(await r.text());
  } catch (err) {
    res.status(502).json({ error: "upstream_failed" });
  }
}
