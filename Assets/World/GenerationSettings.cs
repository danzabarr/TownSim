using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="GenerationSettings", menuName ="Generation/Settings")]
public class GenerationSettings : ScriptableObject
{
    private static Dictionary<string, GenerationSettings> settings;
    public static Dictionary<string, GenerationSettings> Settings
    {
        get
        {
            if (settings == null)
            {
                settings = new Dictionary<string, GenerationSettings>();
                GenerationSettings[] types = Resources.LoadAll<GenerationSettings>("Generation Settings");

                foreach (GenerationSettings type in types)
                {
                    if (!settings.ContainsKey(type.name))
                        settings.Add(type.name, type);
                }
            }

            return settings;
        }
    }

    public static string[] SettingsNames => new List<string>(Settings.Keys).ToArray();

    public static bool Load(string name, out GenerationSettings settings)
    {
        if (name == null)
        {
            settings = null;
            return false;
        }

        return Settings.TryGetValue(name, out settings);
    }

    public enum BlendMode
    {
        Additive,
        Subtractive,
        Multiplicative,
        Min,
        Max,
    }

    [System.Serializable]
    public struct NoiseLayerGroup
    {
        public string name;
        public bool foldout;
        [Range(0, 1)]
        public float opacity;
        public BlendMode blending;
        public NoiseLayer[] layers;
    }

    [System.Serializable]
    public struct NoiseLayer
    {
        public string name;
        [Range(0, 1)]
        public float opacity;
        public BlendMode blending;
        public NoiseSettings noise;
    }

    [System.Serializable]
    public struct TreeLayer {
        public string name;
        [Range(0, 1)]
        public float threshold;
        public float minAltitude;
        public float maxAltitude;
        public NoiseSettings noise;
        public Gradient colors;
        public AnimationCurve sizes;
        public Tree prefab;
    }

    [System.Serializable]
    public struct RockLayer
    {
        public string name;
        [Range(0, 1)]
        public float threshold;
        public float minAltitude;
        public float maxAltitude;
        public NoiseSettings noise;
        public Rocks prefab;
    }

    public NoiseLayerGroup[] groups;

    public TreeLayer[] treeLayers;
    public RockLayer[] rockLayers;

    public float Sample(float x, float y)
    {
        float sample = 0;

        foreach(NoiseLayerGroup group in groups)
        {
            float groupSample = 0;
            foreach (NoiseLayer layer in group.layers)
                Apply(layer.blending, layer.opacity, Perlin.Noise(x, y, layer.noise), ref groupSample);

            Apply(group.blending, group.opacity, groupSample, ref sample);
        }

        return sample;
    }

    public static void Apply(BlendMode mode, float opacity, float src, ref float dst)
    {
        switch (mode)
        {
            case BlendMode.Additive:
                dst = Mathf.Lerp(dst, dst + src, opacity);
                break;
            case BlendMode.Subtractive:
                dst = Mathf.Lerp(dst, dst - src, opacity);
                break;
            case BlendMode.Multiplicative:
                dst = Mathf.Lerp(dst, dst * src, opacity);
                break;
            case BlendMode.Min:
                dst = Mathf.Lerp(dst, Mathf.Min(dst, src), opacity);
                break;
            case BlendMode.Max:
                dst = Mathf.Lerp(dst, Mathf.Max(dst, src), opacity);
                break;
        }
    }
}
