using UnityEngine;

[System.Serializable]
public struct NoiseSettings
{
    [SerializeField]
    public Vector2 offset;
    [SerializeField]
    public float frequency;
    [SerializeField]
    [Range(1, 8)]
    public int octaves;
    [SerializeField]
    [Range(1f, 4f)]
    public float lacunarity;
    [SerializeField]
    [Range(0f, 1f)]
    public float persistence;
    [SerializeField]
    public float scale;
    [SerializeField]
    public float height;
    [SerializeField]
    public AnimationCurve resample;

    public NoiseSettings(Vector2 offset, float frequency, int octaves, float lacunarity, float persistence, float scale, float height, AnimationCurve resample)
    {
        this.offset = offset;
        this.frequency = frequency;
        this.octaves = octaves;
        this.lacunarity = lacunarity;
        this.persistence = persistence;
        this.scale = scale;
        this.height = height;
        this.resample = resample;
    }
}
