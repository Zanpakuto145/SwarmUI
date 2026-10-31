# Anima Artist Mixer (SwarmUI extension)

SwarmUI parameter group for [Anima-Artist-Mixer](https://github.com/Zanpakuto145/Anima-Artist-Mixer): **multi-artist style mixing on Anima models**. Anima's LLM text encoder contextualizes all artist tags together, so several artists in one prompt blur or fight each other. The mixer instead encodes each artist separately together with your prompt and mixes them inside the model.

Works on base Anima and Anima-2.9B (both are Swarm model class `anima`).

## Install

1. Copy/clone this folder to `SwarmUI/src/Extensions/AnimaArtistMixer/`, restart SwarmUI.
2. On the Generate tab, open the **Anima Artist Mixer** parameter group and click **Install Anima Artist Mixer** (requires the ComfyUI Self-Starting backend and install permission). Swarm clones the node pack into `DLNodes/Anima-Artist-Mixer` and restarts the backend. No extra Python dependencies.
   - External ComfyUI backends: install the node pack into that ComfyUI's `custom_nodes` yourself.

## Usage

1. Select an Anima model.
2. **Remove the artist tags from your main prompt.**
3. Enable the **Anima Artist Mixer** group and put the artists in **[AAM] Artists**, separated by commas or newlines:
   ```
   wlop
   (krenz:0.8)
   1.2::sakimichan
   ```
   `(name:w)` is a normal prompt weight; `w::name` / `name::w` is a linear mix weight (relative ratio).
4. Generate.

| Parameter | Default | Notes |
|---|---|---|
| Artists | — | Activates the feature. Up to 32 artists. |
| Mixer Mode | Cross-Attn | `Cross-Attn` = established path. `Adapter` = experimental, mixes once after the LLM adapter, ~2x faster. |
| Strength | 1.0 | Cross-Attn + `base_preserve` fusion: try 1.2–1.8 for stronger style. |
| Combine Mode | output_avg | Cross-Attn only. |
| Fusion Mode | interpolate | Cross-Attn only. |
| Alignment Mode | base_anchored | Adapter only. |
| Normalize Weights | true | Adapter only (Cross-Attn always normalizes). |
| Apply To Uncond / Uncond Strength | off | Advanced. |

### How it plugs into Swarm's workflow

Right after Swarm encodes the positive prompt, the extension adds `AnimaArtistPack` (encodes your prompt + each artist with the current text encoder) and `AnimaArtistCrossAttn` / `AnimaArtistAdapterMixer` (patches a clone of the model). The mixer's `base_prompt` conditioning replaces Swarm's positive conditioning, because the model patch is aligned to exactly that encoding. Later conditioning features (image prompts, ControlNet, ...) still stack on top.

### Limits

- Anima models only (other models give a clear error — turn the group off).
- `<region:>` / `<object:>` prompt syntax is not supported while the group is enabled. `<segment:>` and the refiner/upscale stage still run with the artist-patched model.
- The negative prompt is untouched.
- The node pack's "Advanced Options", "Structure Guard" and "Style Balance" nodes are not exposed yet.

## Uninstall

Remove `src/Extensions/AnimaArtistMixer/` (and optionally `DLNodes/Anima-Artist-Mixer/`), restart.

## License

Extension code: MIT. The Anima-Artist-Mixer node pack (by An1X3R) is GPL-3.0 and is downloaded separately, not bundled.
