using TownSim.Units;
using UnityEngine;

public class Tool : MonoBehaviour
{
    protected Human parent;
    public Vector3 endPoint;

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transform.TransformPoint(endPoint), .1f);
    }

    private void Awake()
    {
        parent = GetComponentInParent<Human>();
    }
}
