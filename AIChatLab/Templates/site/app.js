const SYSTEM_PROMPT = {{PERSONA_JS}};
const WELCOME = {{WELCOME_JS}};
const DEFAULT_MODEL = {{DEFAULT_MODEL_JS}};
const ALLOWED_MODELS = {{MODELS_JSON}};
const TEMPERATURE = {{TEMPERATURE}};
const MAX_TOKENS = {{MAX_TOKENS}};
const UI = {{UI_JSON}};
const LS_KEY = "aicl:key";
const LS_MODEL = "aicl:model";
const LS_THREAD = "aicl:thread";
const PREVIEW = window.__AICHATLAB_PREVIEW__ === true;

const threadEl = document.getElementById("thread");
const form = document.getElementById("composer");
const input = document.getElementById("input");
const sendBtn = document.getElementById("btn-send");
const stopBtn = document.getElementById("btn-stop");
const keyModal = document.getElementById("key-modal");
const settingsModal = document.getElementById("settings");
const toastEl = document.getElementById("toast");
const modelSelect = document.getElementById("model-select");
const customModel = document.getElementById("custom-model");

let messages = [];
let abortCtl = null;

function getKey() { try { return localStorage.getItem(LS_KEY) || ""; } catch { return ""; } }
function setKey(v) { try { if (v) localStorage.setItem(LS_KEY, v); else localStorage.removeItem(LS_KEY); } catch {} }
function getModel() { try { return localStorage.getItem(LS_MODEL) || DEFAULT_MODEL; } catch { return DEFAULT_MODEL; } }
function setModel(v) { try { localStorage.setItem(LS_MODEL, v); } catch {} }
function escapeHtml(s) {
  return String(s).replace(/&/g,"&amp;").replace(/</g,"&lt;").replace(/>/g,"&gt;");
}
function renderMarkdown(raw) {
  return escapeHtml(raw)
    .replace(/```([\s\S]*?)```/g, "<pre><code>$1</code></pre>")
    .replace(/`([^`]+)`/g, "<code>$1</code>")
    .replace(/\*\*([^*]+)\*\*/g, "<strong>$1</strong>")
    .replace(/\n/g, "<br>");
}
function addBubble(role, text) {
  const div = document.createElement("div");
  div.className = "bubble " + role;
  div.innerHTML = renderMarkdown(text);
  threadEl.appendChild(div);
  threadEl.scrollTop = threadEl.scrollHeight;
  return div;
}
function toast(msg) {
  toastEl.textContent = msg;
  toastEl.hidden = false;
  clearTimeout(toast._t);
  toast._t = setTimeout(() => { toastEl.hidden = true; }, 4200);
}
function hydrateModels() {
  modelSelect.innerHTML = "";
  const current = getModel();
  const ids = ALLOWED_MODELS.slice();
  if (current && !ids.includes(current)) ids.push(current);
  for (const id of ids) {
    const opt = document.createElement("option");
    opt.value = id; opt.textContent = id;
    if (id === current) opt.selected = true;
    modelSelect.appendChild(opt);
  }
}
function persistThread() { try { localStorage.setItem(LS_THREAD, JSON.stringify(messages)); } catch {} }
function loadThread() {
  try { const p = JSON.parse(localStorage.getItem(LS_THREAD) || "null"); return Array.isArray(p) ? p : null; } catch { return null; }
}
function showKeyModal(force) {
  if (PREVIEW) return;
  if (!force && getKey()) return;
  keyModal.hidden = false;
}
document.getElementById("btn-save-key").addEventListener("click", () => {
  const v = document.getElementById("key-input").value.trim();
  if (!v.startsWith("sk-or-") || v.length < 20) { toast(UI.badKey); return; }
  setKey(v);
  document.getElementById("settings-key").value = v;
  keyModal.hidden = true;
});
document.getElementById("btn-settings").addEventListener("click", () => {
  hydrateModels();
  document.getElementById("settings-key").value = getKey();
  settingsModal.hidden = false;
});
document.getElementById("btn-close-settings").addEventListener("click", () => {
  const custom = customModel.value.trim();
  setModel(custom || modelSelect.value);
  const k = document.getElementById("settings-key").value.trim();
  if (k) setKey(k);
  settingsModal.hidden = true;
});
document.getElementById("btn-clear-key").addEventListener("click", () => { setKey(""); toast(UI.keyCleared); });
document.getElementById("btn-clear-thread").addEventListener("click", () => {
  messages = []; persistThread(); threadEl.innerHTML = "";
  if (WELCOME) addBubble("assistant", WELCOME);
  settingsModal.hidden = true;
});
input.addEventListener("keydown", (e) => {
  if (e.key === "Enter" && !e.shiftKey) { e.preventDefault(); form.requestSubmit(); }
});
stopBtn.addEventListener("click", () => { if (abortCtl) abortCtl.abort(); });
form.addEventListener("submit", async (e) => {
  e.preventDefault();
  const text = input.value.trim();
  if (!text) return;
  if (!PREVIEW && !getKey()) { showKeyModal(true); return; }
  input.value = "";
  messages.push({ role: "user", content: text });
  persistThread();
  addBubble("user", text);
  if (PREVIEW) {
    const echo = UI.previewEcho;
    messages.push({ role: "assistant", content: echo });
    persistThread();
    addBubble("assistant", echo);
    return;
  }
  sendBtn.disabled = true; stopBtn.hidden = false;
  abortCtl = new AbortController();
  const bubble = addBubble("assistant", UI.thinking);
  try {
    const res = await fetch("/api/chat", {
      method: "POST",
      headers: { "Content-Type": "application/json", "x-openrouter-key": getKey() },
      body: JSON.stringify({
        model: getModel(), temperature: TEMPERATURE, max_tokens: MAX_TOKENS,
        messages: [{ role: "system", content: SYSTEM_PROMPT }, ...messages],
      }),
      signal: abortCtl.signal,
    });
    const data = await res.json().catch(() => ({}));
    if (res.status === 401) throw new Error(UI.err401);
    if (res.status === 429) throw new Error(UI.err429);
    if (!res.ok) throw new Error(data.error || UI.err500);
    const content = data.choices?.[0]?.message?.content || "";
    if (!content) throw new Error(UI.err500);
    messages.push({ role: "assistant", content }); persistThread();
    bubble.innerHTML = renderMarkdown(content);
  } catch (err) {
    const msg = err && err.name === "AbortError" ? UI.stopped : (err instanceof Error ? err.message : UI.errNet);
    toast(msg); bubble.innerHTML = renderMarkdown(msg);
  } finally {
    sendBtn.disabled = false; stopBtn.hidden = true; abortCtl = null;
  }
});
(function init() {
  hydrateModels();
  const saved = loadThread();
  if (saved && saved.length) { messages = saved; for (const m of messages) addBubble(m.role, m.content); }
  else if (WELCOME) addBubble("assistant", WELCOME);
  showKeyModal(false);
})();
