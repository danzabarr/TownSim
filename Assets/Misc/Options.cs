using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

public class Options : MonoBehaviour
{
    private PostProcessVolume ppv;
    private Camera main;

    public void SetGrassDistance(float distance)
    {
        if (main == null)
            main = Camera.main;
        bool enableGrass = distance > 0;

        if (enableGrass)
        {
            main.cullingMask |= (1 << LayerMask.NameToLayer("Grass"));

        }
        else
        {
            main.cullingMask &= ~(1 << LayerMask.NameToLayer("Grass"));

        }

        Shader.SetGlobalFloat("_DistanceCulling", distance);
    }

    public void EnablePostProcessing(bool enable)
    {
        if (ppv == null)
            ppv = FindObjectOfType<PostProcessVolume>();
        ppv.enabled = enable;
    }

    public void EnableAmbientOcclusion(bool enabled)
    {
        if (ppv == null)
            ppv = FindObjectOfType<PostProcessVolume>();
        ppv.profile.GetSetting<AmbientOcclusion>().enabled.value = enabled;
    }

    public void EnableBloom(bool enabled)
    {
        if (ppv == null)
            ppv = FindObjectOfType<PostProcessVolume>();
        ppv.profile.GetSetting<Bloom>().enabled.value = enabled;
    }

    public void EnableDepthOfField(bool enabled)
    {
        if (ppv == null)
            ppv = FindObjectOfType<PostProcessVolume>();
        ppv.profile.GetSetting<DepthOfField>().enabled.value = enabled;
    }

    public void EnableChromaticAberration(bool enabled)
    {
        if (ppv == null)
            ppv = FindObjectOfType<PostProcessVolume>();
        ppv.profile.GetSetting<ChromaticAberration>().enabled.value = enabled;
    }
}
