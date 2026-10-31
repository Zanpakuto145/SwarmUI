using SwarmUI.Builtin_ComfyUIBackend;
using SwarmUI.Core;
using SwarmUI.Text2Image;
using SwarmUI.Utils;

namespace Zanpakuto145.SwarmExtensions.RES4LYFSamplers;

/// <summary>Adds the RES4LYF samplers (res_2m, res_2s, deis_2m, ...) and schedulers (bong_tangent, beta57) to SwarmUI's Sampler/Scheduler dropdowns, by auto-installing the RES4LYF node pack into the self-start ComfyUI backend.</summary>
public class RES4LYFSamplers : Extension
{
    /// <summary>Installer ID for the backing ComfyUI node pack.</summary>
    public const string FeatureID = "res4lyf";

    /// <summary>Repository holding the ComfyUI node pack.</summary>
    public const string NodeRepoURL = "https://github.com/Zanpakuto145/RES4LYF";

    public override void OnInit()
    {
        InstallableFeatures.RegisterInstallableFeature(new("RES4LYF", FeatureID, NodeRepoURL, "ClownsharkBatwing",
            "This will install RES4LYF developed by ClownsharkBatwing.\nNote its license forbids use in a commercial generation service without permission.\nDo you wish to install?", AutoInstall: true));
        ComfyUIBackendExtension.NodeToFeatureMap["ClownsharKSampler_Beta"] = FeatureID;
        // AutoInstall at backend start does not run requirements.txt. Of RES4LYF's requirements, only PyWavelets is not already provided by Swarm
        // (Swarm ships opencv-python-headless, matplotlib and numpy; installing the non-headless opencv-python alongside would risk a cv2 conflict).
        // Without it the beta samplers (res_2m etc.) silently fail to import, so make Swarm's own dependency check install it.
        if (!ComfyUISelfStartBackend.RequiredPythonPackages.Any(p => p.Item1 == "pywt"))
        {
            ComfyUISelfStartBackend.RequiredPythonPackages.Add(("pywt", "PyWavelets"));
        }
        // Friendly display names. These are only labels: the values are exactly what RES4LYF registers with comfy's KSampler lists.
        T2IParamTypes.ConcatDropdownValsClean(ref ComfyUIBackendExtension.Samplers, [
            "res_2m///RES 2M (RES4LYF, Multistep)", "res_3m///RES 3M (RES4LYF, Multistep)",
            "res_2s///RES 2S (RES4LYF, 2x Slow)", "res_3s///RES 3S (RES4LYF, 3x Slow)", "res_5s///RES 5S (RES4LYF, 5x Slow)", "res_6s///RES 6S (RES4LYF, 6x Slow)",
            "res_2m_ode///RES 2M ODE (RES4LYF)", "res_3m_ode///RES 3M ODE (RES4LYF)",
            "res_2s_ode///RES 2S ODE (RES4LYF, 2x Slow)", "res_3s_ode///RES 3S ODE (RES4LYF, 3x Slow)", "res_5s_ode///RES 5S ODE (RES4LYF, 5x Slow)", "res_6s_ode///RES 6S ODE (RES4LYF, 6x Slow)",
            "deis_2m///DEIS 2M (RES4LYF)", "deis_3m///DEIS 3M (RES4LYF)", "deis_2m_ode///DEIS 2M ODE (RES4LYF)", "deis_3m_ode///DEIS 3M ODE (RES4LYF)",
            "rk_beta///RK Beta (RES4LYF)"
        ]);
        T2IParamTypes.ConcatDropdownValsClean(ref ComfyUIBackendExtension.Schedulers, ["bong_tangent///Bong Tangent (RES4LYF)", "beta57///Beta 57 (RES4LYF, beta a=0.5 b=0.7)"]);
        Logs.Init("RES4LYF Samplers extension loaded: RES4LYF will be auto-installed into the self-start ComfyUI backend.");
    }
}
