using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : Tool
{
    public float damage;
    public float knockback;
    [Range(0, 1)] public float knockbackDirectionAtten;
    [Range(0, 1)] public float knockbackDistanceAtten;
    public float recoilSpeed = 1;

    public Vector3[] hitVolumes;
    public float volumeRadius;

    private Vector3[] lastPos;

    private void FixedUpdate()
    {
        if (!parent.MeleeDamageActive)
            return;

        if (lastPos == null)
        {
            lastPos = new Vector3[hitVolumes.Length];
            for (int i = 0; i < hitVolumes.Length; i++)
                lastPos[i] = transform.TransformPoint(hitVolumes[i]);
        }

        ITarget nearest = null;
        float nearestT = 0;
        Vector3 nearestPos = default;

        for (int i = 0; i < hitVolumes.Length; i++)
        {
            Vector3 p0 = lastPos[i];
            Vector3 p1 = transform.TransformPoint(hitVolumes[i]);

            Vector3 d = (p1 - p0).normalized;
            float len = (p1 - p0).magnitude;

            if (Physics.SphereCast(p0, volumeRadius, d, out RaycastHit hit, len))
            {
                ITarget target = hit.collider.GetComponentInParent<ITarget>();
                if (target != null)
                {
                    float t = hit.distance / len;
                    if (nearest == null || t < nearestT)
                    {
                        nearest = target;
                        nearestT = t;
                        nearestPos = hit.point;
                    }
                }


            }

            lastPos[i] = p1;
        }
        if (nearest != null)
        {
            parent.MeleeHit(nearest, nearestPos, damage, knockback, knockbackDirectionAtten, knockbackDistanceAtten, recoilSpeed);
        }
    }

    private void OnDrawGizmosSelected()
    {
        foreach (Vector3 hv in hitVolumes)
            Gizmos.DrawSphere(transform.TransformPoint(hv), volumeRadius);
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (!parent.MeleeDamageActive)
    //        return;
    //
    //    ITarget target = other.GetComponent<ITarget>();
    //
    //    if (target != null)
    //    {
    //        parent.MeleeHit(target, transform.TransformPoint(endPoint));
    //    }
    //}

}
