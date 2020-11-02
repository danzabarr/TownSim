using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Axe : Tool
{
    public int effectiveness = 1;
    public float recoilSpeed = 1;
    private void OnTriggerEnter(Collider other)
    {
        if (!parent.AxeActive)
            return;

        Tree tree = other.GetComponentInParent<Tree>();

        if (tree != null && !tree.Fallen)
        {
            parent.target = tree;

            Vector3 direction = tree.transform.position - parent.transform.position;
            direction.y = 0;
            direction = direction.normalized;

            tree.Chop(effectiveness, transform.TransformPoint(endPoint), direction * 1f);
            return;
        }

        TreeStump stump = other.GetComponentInParent<TreeStump>();

        if (stump != null && stump.Fallen)
        {
            Destroy(stump.gameObject);
            return;
        }

        ITarget target = other.GetComponentInParent<ITarget>();
        if (target != null && target.RecoilsFrom(parent.Mode, out float recoilSpeedModifier))
            parent.Recoil(transform.TransformPoint(endPoint), recoilSpeed * recoilSpeedModifier);
    }
}
