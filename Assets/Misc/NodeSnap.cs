using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-2)]
public class NodeSnap : MonoBehaviour
{
    public void Start()
    {
        Snap();
    }

    public void Snap()
    {
        transform.position = HexUtils.SnapToVert(transform.position.x, transform.position.z, Map.Size, Map.NodeRes).xny(transform.position.y);
    }
}
