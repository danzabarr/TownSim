using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameObjectExtensions
{
    public static void SetLayerRecursively(this GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform child in go.transform)
            child.gameObject.SetLayerRecursively(layer);
    }

    public static void SetLayerRecursively(this GameObject go, LayerMask layers, int layer)
    {
        if (layers.Contains(go.layer))
            go.layer = layer;

        foreach (Transform child in go.transform)
            child.gameObject.SetLayerRecursively(layers, layer);
    }

    public static bool Contains(this LayerMask mask, int layer)
    {
        return mask == (mask | (1 << layer));
    }

    public static void SetActiveRecursively(this GameObject go, LayerMask layers, bool value)
    {
        if (layers.Contains(go.layer))
            go.SetActive(value);

        foreach (Transform child in go.transform)
            child.gameObject.SetActiveRecursively(layers, value);
    }
}
