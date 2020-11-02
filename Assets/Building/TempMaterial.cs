using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempMaterial : MonoBehaviour
{
    private new Renderer renderer;
    private Material[] materials;
    public bool useInstancedMaterials;
    public bool UsingTempMaterials { get; private set; }
    public bool Cached { get; private set; }

    public void CacheMaterials()
    {

        if (renderer == null)
            renderer = GetComponent<Renderer>();

        if (Cached)
            return;

        if (UsingTempMaterials)
            return;

        Debug.Log("Caching materials for " + gameObject);

        materials = useInstancedMaterials ? renderer.materials : renderer.sharedMaterials;
        Cached = true;
    }

    public void SetMaterial(Material material, bool cacheFirst = true)
    {
        if (cacheFirst)
            CacheMaterials();

        Debug.Log("Setting material to " + material.name + " for " + gameObject);
        if (useInstancedMaterials)
            renderer.materials = new Material[] { material };
        else
            renderer.sharedMaterials = new Material[] { material };
        UsingTempMaterials = true;
    }

    public void RevertToCached(bool clear = true)
    {
        if (!Cached)
            throw new System.Exception("Reverting to materials not cached.");

        Debug.Log("Reverting " + gameObject + " to cached materials");
        if (useInstancedMaterials)
            renderer.materials = materials;
        else
            renderer.sharedMaterials = materials;

        if (clear)
            ClearCache();

        UsingTempMaterials = false;
    }

    public void ClearCache()
    {
        if (!Cached)
            return;

        Debug.Log("Clearing cache for " + gameObject);
        materials = null;
        Cached = false;
    }

    public static void CacheAll(GameObject go)
    {
        foreach (TempMaterial tm in go.GetComponentsInChildren<TempMaterial>())
            tm.CacheMaterials();
    }

    public static void SetAll(GameObject go, Material material, bool cacheFirst = true)
    {
        foreach (TempMaterial tm in go.GetComponentsInChildren<TempMaterial>())
            tm.SetMaterial(material, cacheFirst);
    }

    public static void RevertAll(GameObject go, bool clear = true)
    {
        foreach (TempMaterial tm in go.GetComponentsInChildren<TempMaterial>())
            tm.RevertToCached(clear);
    }

    public static void ClearAll(GameObject go)
    {
        foreach (TempMaterial tm in go.GetComponentsInChildren<TempMaterial>())
            tm.ClearCache();
    }
}
