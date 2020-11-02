using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickaxe : Tool
{
    public float dropChanceModifier = 1;
    public float recoilSpeed = 1;
    private void OnTriggerEnter(Collider other)
    {
        if (!parent.PickaxeActive)
            return;

        Rocks rocks = other.GetComponentInParent<Rocks>();

        if (rocks != null)
        {
            parent.target = rocks;
            rocks.Mine(dropChanceModifier, transform.TransformPoint(endPoint));
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
