# Anima 2.9B LoRA Patch (SwarmUI extension)

Makes **base Anima LoRAs** (trained on anima-base-v1.0 / anima-preview, 28 blocks) apply to the **correct layers of Anima-2.9B models** (40 blocks) in SwarmUI.

Anima-2.9B is a depth expansion of the base model: 12 new blocks were inserted between the original 28, so base block `j` no longer sits at index `j`. Without remapping, a base LoRA silently lands on the wrong layers from block 2 onward (no "key not loaded" warning) — it "doesn't work" or degrades the image as strength goes up.

This extension auto-installs [ComfyUI-Anima-2.9B-loraPatch](https://github.com/Zanpakuto145/ComfyUI-Anima-2.9B-loraPatch) into Swarm's ComfyUI backend. That patch wraps ComfyUI's `comfy.lora.load_lora` and rewrites block indices at load time, so **SwarmUI's normal LoRA selector just works** — no new parameters, no workflow changes.

## Why this one and not ComfyUI-Anima-Remap?

ComfyUI-Anima-Remap is a set of *workflow nodes* (its own LoRA tag loader, merge nodes, ...). SwarmUI builds its own workflows and loads LoRAs with the standard ComfyUI `LoraLoader`, so Remap's nodes would never be used without replacing Swarm's LoRA handling. The loraPatch hooks `comfy.lora.load_lora`, the function ComfyUI's LoRA loaders (including the ones Swarm emits) go through, so it works transparently. It is also self-gating: it only remaps when the model has exactly 40 Anima blocks *and* the LoRA covers exactly blocks 0..27. Everything else (28-block models, native 2.9B LoRAs, non-Anima models) passes through untouched.

## Requirements

- SwarmUI with the **ComfyUI Self-Starting** backend (the patch is auto-installed there).
- Anima-2.9B must load with all 40 blocks: needs a ComfyUI version that includes ComfyUI PR #15555 (merged 2026-08-12). Swarm keeps its ComfyUI on recent `master`, so update ComfyUI via Swarm (Server → Update / backend update) if your 2.9B model loads as a 28-block model.
- Externally managed ComfyUI (API-by-URL backend): install the patch yourself into that ComfyUI's `custom_nodes` and restart it.

## Install

1. Copy/clone this folder to `SwarmUI/src/Extensions/Anima29BLoraPatch/`.
2. Restart SwarmUI (rebuilds extensions). The backend will clone the patch into `src/BuiltinExtensions/ComfyUIBackend/DLNodes/ComfyUI-Anima-2.9B-loraPatch/` on startup.
3. Check the ComfyUI backend log for:
   ```
   [Anima 2.9B LoRA Patch] installed (base block j -> 2.9B block [0, 1, 3, 4, 6, 7]...)
   ```
   and, when generating with a base LoRA on a 2.9B model:
   ```
   [Anima 2.9B LoRA Patch] 28-block LoRA on 40-block Anima-2.9B: remapped N target keys ...
   ```

## Usage

Select an Anima-2.9B model and add base Anima LoRAs as usual. Start around LoRA weight **0.5–0.7**: layer correspondence is now correct, but the residual stream passes through 12 new blocks and a retrained adapter, so the LoRA won't behave identically to how it does on the base model.

To disable the remap without uninstalling, set the environment variable `ANIMA_LORA_REMAP=0` in the environment that launches SwarmUI (the self-started ComfyUI inherits it).

## Uninstall

Remove `src/Extensions/Anima29BLoraPatch/` **and** `src/BuiltinExtensions/ComfyUIBackend/DLNodes/ComfyUI-Anima-2.9B-loraPatch/`, then restart.

## License

Extension code: MIT. The installed ComfyUI patch (by sparklingcoffee777) is GPL-3.0 and is downloaded separately, not bundled.
