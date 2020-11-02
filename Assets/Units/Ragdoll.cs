using UnityEngine;

[DisallowMultipleComponent]
public class Ragdoll : MonoBehaviour
{
    public Animator animator;
    public Rigidbody rb;
    public Collider[] colliders;
    public Rigidbody[] ragdoll;

    [ContextMenu("Activate")]
    public void Activate() => Activate(true);

    public void Activate(bool enable)
    {
        if (rb)
            rb.isKinematic = enable;

        foreach(Collider c in colliders)
            c.enabled = !enable;

        if (animator)
            animator.enabled = !enable;

        foreach (Rigidbody rd in ragdoll)
            rd.isKinematic = !enable;
    }

    public void AddForce(Vector3 force, ForceMode mode)
    {
        foreach (Rigidbody rd in ragdoll)
            rd.AddForce(force, mode);
    }

    public void AddForceAtPosition(Vector3 force, Vector3 position, ForceMode mode)
    {
        foreach (Rigidbody rd in ragdoll)
            rd.AddForceAtPosition(force, position, mode);
    }
}
