using SwarmUI.Core;
using SwarmUI.Utils;

namespace Zanpakuto145.SwarmExtensions.Anima29BLoraPatch;

/// <summary>Makes base Anima (28-block) LoRAs apply to the correct layers of Anima-2.9B (40-block) models, by auto-installing the ComfyUI-Anima-2.9B-loraPatch node pack into Swarm's self-start ComfyUI backend.</summary>
public class Anima29BLoraPatch : Extension
{
    /// <summary>Installer ID for the backing ComfyUI patch.</summary>
    public const string FeatureID = "anima_29b_lora_patch";

    /// <summary>Repository that holds the ComfyUI load-time patch.</summary>
    public const string PatchRepoURL = "https://github.com/Zanpakuto145/ComfyUI-Anima-2.9B-loraPatch";

    public override void OnInit()
    {
        // The patch has no workflow nodes: it wraps comfy.lora.load_lora at ComfyUI startup, so Swarm's ordinary LoRA loader nodes pick it up automatically.
        // AutoInstall clones it into DLNodes on every self-start backend launch (if missing), and Swarm's managed-node updater keeps it current.
        InstallableFeatures.RegisterInstallableFeature(new("Anima 2.9B LoRA Patch", FeatureID, PatchRepoURL, "sparklingcoffee777",
            "This will install the Anima 2.9B LoRA Patch, which remaps base Anima (28-block) LoRAs onto Anima-2.9B (40-block) models.\nDo you wish to install?", AutoInstall: true));
        Logs.Init("Anima 2.9B LoRA Patch extension loaded: ComfyUI-Anima-2.9B-loraPatch will be auto-installed into the self-start ComfyUI backend.");
    }
}
