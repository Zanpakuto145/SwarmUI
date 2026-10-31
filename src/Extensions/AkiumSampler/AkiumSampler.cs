using SwarmUI.Builtin_ComfyUIBackend;
using SwarmUI.Core;
using SwarmUI.Text2Image;
using SwarmUI.Utils;

namespace Zanpakuto145.SwarmExtensions.AkiumSampler;

/// <summary>Adds the Akium momentum-guided samplers to SwarmUI's Sampler dropdown, by auto-installing the akium-sampler-comfy node pack into the self-start ComfyUI backend.</summary>
public class AkiumSampler : Extension
{
    /// <summary>Installer ID for the backing ComfyUI node pack.</summary>
    public const string FeatureID = "akium_sampler";

    /// <summary>Repository holding the ComfyUI node pack.</summary>
    public const string NodeRepoURL = "https://github.com/Zanpakuto145/akium-sampler-comfy";

    public override void OnInit()
    {
        // The pack appends 'akium' and 'akium_color' to comfy's KSampler sampler list on load, which SwarmKSampler (and thus Swarm's Sampler param) already reads.
        // No requirements.txt, so AutoInstall at backend start is safe.
        InstallableFeatures.RegisterInstallableFeature(new("Akium Sampler", FeatureID, NodeRepoURL, "AkiumAI",
            "This will install the Akium Sampler (CC-BY-4.0) developed by AkiumAI.\nDo you wish to install?", AutoInstall: true));
        ComfyUIBackendExtension.NodeToFeatureMap["AkiumSampler"] = FeatureID;
        // Friendly display names. These are only labels: the values are exactly what the pack registers with comfy.
        T2IParamTypes.ConcatDropdownValsClean(ref ComfyUIBackendExtension.Samplers, ["akium///Akium (Momentum Guided, 2x Slow)", "akium_color///Akium Color (Momentum Guided + Chroma Boost, 2x Slow)"]);
        Logs.Init("Akium Sampler extension loaded: akium-sampler-comfy will be auto-installed into the self-start ComfyUI backend.");
    }
}
