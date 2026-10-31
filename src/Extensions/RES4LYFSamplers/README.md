# RES4LYF Samplers (SwarmUI extension)

Adds the [RES4LYF](https://github.com/Zanpakuto145/RES4LYF) samplers and schedulers to SwarmUI's normal **Sampler** / **Scheduler** dropdowns:

- Samplers: `res_2m`, `res_3m`, `res_2s`, `res_3s`, `res_5s`, `res_6s`, their `_ode` (deterministic) variants, `deis_2m`, `deis_3m` (+ `_ode`), `rk_beta`. The `Ns` samplers cost N model calls per step.
- Schedulers: `bong_tangent`, `beta57`.

RES4LYF's own ClownsharKSampler/guide nodes are installed too (usable in the Comfy Workflow tab), but Swarm's Generate tab uses them only through the samplers above.

## Install

Copy/clone this folder to `SwarmUI/src/Extensions/RES4LYFSamplers/` and restart SwarmUI. RES4LYF is auto-installed into the ComfyUI Self-Starting backend (`DLNodes/RES4LYF`) on startup.

Dependencies: Swarm already provides OpenCV (headless), matplotlib and numpy. The extension adds **PyWavelets** to Swarm's required-package check so it is installed on backend start (without it RES4LYF's `res_*` samplers silently fail to load). RES4LYF's `requirements.txt` is intentionally *not* run, because it would install the non-headless `opencv-python` next to Swarm's `opencv-python-headless`.

External ComfyUI backends: install RES4LYF and `PyWavelets` into that ComfyUI yourself.

## Compatibility notes

- No node-ID collisions with SwarmUI's own nodes, Anima-Artist-Mixer or Akium (checked).
- RES4LYF reloads `comfy.k_diffusion.sampling` after registering its samplers; Python's reload keeps attributes added by other packs (e.g. Akium), so both coexist.
- RES4LYF's deprecated `updatedTimestepScaling` option patches `time_snr_shift`; it is off by default — leave it off.

## License

Extension code: MIT. **RES4LYF (by ClownsharkBatwing) has its own license that forbids use for a commercial service (e.g. a paid generation service) without permission** — read it before using RES4LYF commercially. It is downloaded separately, not bundled.
