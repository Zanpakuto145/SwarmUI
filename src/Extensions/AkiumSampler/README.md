# Akium Sampler (SwarmUI extension)

Adds the [Akium](https://github.com/Zanpakuto145/akium-sampler-comfy) momentum-guided samplers to SwarmUI's **Sampler** dropdown:

- **Akium** (`akium`) — second-order stochastic sampler with momentum + look-ahead (2 model calls per step). Sharper fine detail than pure SDE samplers. Tested by its author on Anima and Illustrious.
- **Akium Color** (`akium_color`) — same, plus a subtle chroma boost (tuned for 4-channel latents; effect differs on Anima's Qwen-Image VAE).

## Install

Copy/clone this folder to `SwarmUI/src/Extensions/AkiumSampler/` and restart SwarmUI. The node pack is auto-installed into the ComfyUI Self-Starting backend (`DLNodes/akium-sampler-comfy`) on startup; it has no Python dependencies. External ComfyUI backends: install it into that ComfyUI's `custom_nodes` yourself.

Check the backend log for `[Akium] Registered 'akium' and 'akium_color' in the KSampler dropdown.`

## Usage

Sampling → Sampler → **Akium** or **Akium Color**. Any scheduler. Works together with the Anima LoRA patch, the Artist Mixer and RES4LYF (different sampler names, no shared state).

## License

Extension code: MIT. Akium (by AkiumAI) is CC-BY-4.0 and is downloaded separately, not bundled.
