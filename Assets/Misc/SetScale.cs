using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetScale : MonoBehaviour
{
    public Transform[] transforms;
    public Vector3 scale = Vector3.one;

    private void LateUpdate()
    {
        foreach (Transform t in transforms)
            t.localScale = scale;
    }
}
