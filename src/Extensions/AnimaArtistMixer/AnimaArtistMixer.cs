using Newtonsoft.Json.Linq;
using SwarmUI.Builtin_ComfyUIBackend;
using SwarmUI.Core;
using SwarmUI.Text2Image;
using SwarmUI.Utils;

namespace Zanpakuto145.SwarmExtensions.AnimaArtistMixer;

/// <summary>Exposes the Anima-Artist-Mixer ComfyUI nodes as a SwarmUI parameter group: each artist is encoded separately with the main prompt and mixed inside the model, avoiding artist-tag interference in Anima's LLM text encoder.</summary>
public class AnimaArtistMixer : Extension
{
    /// <summary>Installer ID and discovered feature ID (kept identical, single-pack feature).</summary>
    public const string FeatureID = "anima_artist_mixer";

    /// <summary>Repository holding the ComfyUI node pack.</summary>
    public const string NodeRepoURL = "https://github.com/Zanpakuto145/Anima-Artist-Mixer";

    /// <summary>Parameter group for all of this extension's controls.</summary>
    public static T2IParamGroup MixerGroup;

    /// <summary>Artist chain (comma/newline separated). Presence of this parameter activates the feature.</summary>
    public static T2IRegisteredParam<string> Artists;

    /// <summary>Which mixing algorithm/node to use.</summary>
    public static T2IRegisteredParam<string> MixerMode;

    /// <summary>Overall artist mixing strength.</summary>
    public static T2IRegisteredParam<double> Strength;

    /// <summary>Cross-Attn only: how per-artist outputs are combined.</summary>
    public static T2IRegisteredParam<string> CombineMode;

    /// <summary>Cross-Attn only: how the combined artist output is fused with the base output.</summary>
    public static T2IRegisteredParam<string> FusionMode;

    /// <summary>Adapter only: how artist post-adapter rows are aligned to the base prompt rows.</summary>
    public static T2IRegisteredParam<string> AlignmentMode;

    /// <summary>Adapter only: whether to normalize ::weights to relative ratios.</summary>
    public static T2IRegisteredParam<bool> NormalizeWeights;

    /// <summary>Whether to also inject artists into the unconditional (negative) pass.</summary>
    public static T2IRegisteredParam<bool> ApplyToUncond;

    /// <summary>Strength of the uncond injection, when enabled.</summary>
    public static T2IRegisteredParam<double> UncondStrength;

    public override void OnInit()
    {
        InstallableFeatures.RegisterInstallableFeature(new("Anima Artist Mixer", FeatureID, NodeRepoURL, "An1X3R",
            "This will install the Anima Artist Mixer ComfyUI nodes (GPL-3.0) developed by An1X3R.\nDo you wish to install?"));
        ComfyUIBackendExtension.NodeToFeatureMap["AnimaArtistPack"] = FeatureID;
        ComfyUIBackendExtension.NodeToFeatureMap["AnimaArtistCrossAttn"] = FeatureID;
        ComfyUIBackendExtension.NodeToFeatureMap["AnimaArtistAdapterMixer"] = FeatureID;
        ScriptFiles.Add("Assets/anima_artist_mixer.js");
        MixerGroup = new("Anima Artist Mixer", Toggles: true, Open: false, IsAdvanced: false, OrderPriority: 9,
            Description: "Mix multiple artist styles on Anima models without the artist tags interfering with each other inside the LLM text encoder.\nEach artist is encoded separately together with your main prompt, then mixed inside the model.\nRemove the artist tags from your main prompt and list them here instead.");
        Artists = T2IParamTypes.Register<string>(new("[AAM] Artists", "Artist chain, separated by commas or newlines.\nSupports prompt weights like '(wlop:1.2)' and linear mix weights like '1.5::wlop' or 'wlop::1.5'.\nUp to 32 artists. Do not also put these artists in the main prompt.",
            "", Group: MixerGroup, FeatureFlag: FeatureID, OrderPriority: 1, ViewType: ParamViewType.BIG, Examples: ["wlop, krenz", "1.2::sakimichan\nwlop"]));
        MixerMode = T2IParamTypes.Register<string>(new("[AAM] Mixer Mode", "Which mixing algorithm to use.\n'Cross-Attn' is the established, stable path that mixes inside every cross-attention layer.\n'Adapter' (experimental) mixes once after the LLM adapter; roughly twice as fast with close visual quality.",
            "crossattn", Group: MixerGroup, FeatureFlag: FeatureID, OrderPriority: 2, GetValues: _ => ["crossattn///Cross-Attn (Stable)", "adapter///Adapter (Experimental, Faster)"]));
        Strength = T2IParamTypes.Register<double>(new("[AAM] Strength", "Strength of the artist mixing.\n1.0 is the recommended start. With Cross-Attn 'base_preserve' fusion, 1.2-1.8 gives stronger style.",
            "1", Min: 0, Max: 4, Step: 0.05, ViewMax: 2, Group: MixerGroup, FeatureFlag: FeatureID, OrderPriority: 3, ViewType: ParamViewType.SLIDER, Examples: ["0.5", "1", "1.5"]));
        CombineMode = T2IParamTypes.Register<string>(new("[AAM] Combine Mode", "(Cross-Attn mode only) How per-artist attention outputs are combined.\n'output_avg' is recommended.",
            "output_avg", Group: MixerGroup, FeatureFlag: FeatureID, OrderPriority: 4, GetValues: _ => ["output_avg", "concat", "lowrank_avg"]));
        FusionMode = T2IParamTypes.Register<string>(new("[AAM] Fusion Mode", "(Cross-Attn mode only) How the combined artist output is fused with the base output.\n'interpolate' is recommended; 'base_preserve' for stronger style with higher strength.",
            "interpolate", Group: MixerGroup, FeatureFlag: FeatureID, OrderPriority: 5, GetValues: _ => ["interpolate", "base_preserve", "concat_with_base"]));
        AlignmentMode = T2IParamTypes.Register<string>(new("[AAM] Alignment Mode", "(Adapter mode only) How artist rows are aligned to the base prompt after the LLM adapter.\n'base_anchored' (recommended) keeps every artist's full input and aligns by T5 token IDs.\n'shared_base_ids' is the older common-grid mode.",
            "base_anchored", Group: MixerGroup, FeatureFlag: FeatureID, OrderPriority: 6, GetValues: _ => ["base_anchored", "shared_base_ids"]));
        NormalizeWeights = T2IParamTypes.Register<bool>(new("[AAM] Normalize Weights", "(Adapter mode only) Normalize '::' artist weights to relative ratios.\n(Cross-Attn mode always normalizes.)",
            "true", Group: MixerGroup, FeatureFlag: FeatureID, OrderPriority: 7));
        ApplyToUncond = T2IParamTypes.Register<bool>(new("[AAM] Apply To Uncond", "Also inject artists into the unconditional (negative) pass.\nNormally leave off. In Adapter 'base_anchored' mode the negative pass is always preserved.",
            "false", Group: MixerGroup, FeatureFlag: FeatureID, OrderPriority: 8, IsAdvanced: true));
        UncondStrength = T2IParamTypes.Register<double>(new("[AAM] Uncond Strength", "Scale of artist injection on the unconditional pass when 'Apply To Uncond' is enabled.\nIf not set, defaults to 1.0 for Cross-Attn and 0.0 for Adapter (matching the node defaults).",
            "1", Min: 0, Max: 1, Step: 0.05, Toggleable: true, Group: MixerGroup, FeatureFlag: FeatureID, OrderPriority: 9, IsAdvanced: true, ViewType: ParamViewType.SLIDER));
        // Priority -7.9: right after the core positive prompt is encoded (-8), before ReVision/IPAdapter/etc. (-7) and the main sampler (-5), so later prompt modifiers stack on top of our base conditioning.
        WorkflowGenerator.AddStep(g =>
        {
            if (!g.UserInput.TryGet(Artists, out string artists) || string.IsNullOrWhiteSpace(artists) || g.UserInput.Get(T2IParamTypes.JustLoadModel, false))
            {
                return;
            }
            if (!g.Features.Contains(FeatureID))
            {
                throw new SwarmUserErrorException("Anima Artist Mixer parameters are set, but the Anima Artist Mixer ComfyUI nodes are not installed. Use the install button in the 'Anima Artist Mixer' parameter group.");
            }
            if (g.CurrentCompatClass() != "anima")
            {
                throw new SwarmUserErrorException($"Anima Artist Mixer only works with Anima models, but the current model class is '{g.CurrentCompatClass() ?? "unknown"}'. Disable the 'Anima Artist Mixer' group for this model.");
            }
            PromptRegion region = new(g.UserInput.Get(T2IParamTypes.Prompt, ""));
            if (region.Parts.Any(p => p.Type == PromptRegion.PartType.Region || p.Type == PromptRegion.PartType.Object))
            {
                throw new SwarmUserErrorException("Anima Artist Mixer does not support <region:> or <object:> prompt syntax. Remove those, or disable the 'Anima Artist Mixer' group.");
            }
            // Mirror the core base-stage prompt text selection (global prompt plus any <base> section).
            string basePrompt = region.GlobalPrompt ?? "";
            if (!string.IsNullOrWhiteSpace(region.BasePrompt))
            {
                basePrompt = $"{basePrompt} {region.BasePrompt}";
            }
            string packNode = g.CreateNode("AnimaArtistPack", new JObject()
            {
                ["clip"] = g.CurrentTextEnc.Path,
                ["artist_chain"] = artists,
                ["base_prompt"] = basePrompt.Trim()
            });
            string mode = g.UserInput.Get(MixerMode, "crossattn");
            bool applyUncond = g.UserInput.Get(ApplyToUncond, false);
            string mixerNode;
            if (mode == "adapter")
            {
                mixerNode = g.CreateNode("AnimaArtistAdapterMixer", new JObject()
                {
                    ["model"] = g.CurrentModel.Path,
                    ["artist_pack"] = WorkflowGenerator.NodePath(packNode, 0),
                    ["strength"] = g.UserInput.Get(Strength, 1.0),
                    ["normalize_weights"] = g.UserInput.Get(NormalizeWeights, true),
                    ["alignment_mode"] = g.UserInput.Get(AlignmentMode, "base_anchored"),
                    ["enabled"] = true,
                    ["apply_to_uncond"] = applyUncond,
                    ["uncond_strength"] = g.UserInput.Get(UncondStrength, 0.0)
                });
            }
            else
            {
                mixerNode = g.CreateNode("AnimaArtistCrossAttn", new JObject()
                {
                    ["model"] = g.CurrentModel.Path,
                    ["artist_pack"] = WorkflowGenerator.NodePath(packNode, 0),
                    ["combine_mode"] = g.UserInput.Get(CombineMode, "output_avg"),
                    ["fusion_mode"] = g.UserInput.Get(FusionMode, "interpolate"),
                    ["strength"] = g.UserInput.Get(Strength, 1.0),
                    ["enabled"] = true,
                    ["apply_to_uncond"] = applyUncond,
                    ["uncond_strength"] = g.UserInput.Get(UncondStrength, 1.0)
                });
            }
            // The mixer's base_prompt output is the pack's own encoding of the base text; the model patch is aligned to exactly this conditioning, so it must feed the sampler.
            g.CurrentModel = g.CurrentModel.WithPath([mixerNode, 0]);
            g.FinalPrompt = [mixerNode, 1];
        }, -7.9);
    }
}
