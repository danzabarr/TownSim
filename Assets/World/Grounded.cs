using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class Grounded : MonoBehaviour
{
    public Vector3 samplePosition;
    public float offset;
    public bool useMesh;
    public bool groundOnStart;

    [ContextMenu("Ground")]
    public void Ground()
    {
        Vector3 sample = transform.TransformPoint(samplePosition);
        float y = useMesh ? Map.Instance.MeshHeight(sample.xz()) : Map.Instance.Height(sample.xz()) + offset;
        transform.position = new Vector3(transform.position.x, y, transform.position.z);
    }

    private void Start()
    {
        Ground();
    }
}
