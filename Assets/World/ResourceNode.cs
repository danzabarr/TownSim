using System.Collections;
using System.Collections.Generic;
using TownSim.Items;
using UnityEngine;

public class ResourceNode : MonoBehaviour, ITarget
{
    public float maxHealth;
    public float currentHealth;

    public void FillHealth()
    {
        currentHealth = maxHealth;
    }

    public Vector3 Center()
    {
        return transform.position;
    }

    public float CurrentHealth()
    {
        return currentHealth;
    }

    public float Damage(float amount)
    {
        float damage = Mathf.Min(currentHealth, amount);
        currentHealth -= damage;
        return damage;
    }

    public float Damage(float amount, Vector3 knockbackForce)
    {
        return Damage(amount);
    }

    public float Damage(float amount, Vector3 knockbackOrigin, Vector3 knockbackForce)
    {
        return Damage(amount);
    }

    public int Faction()
    {
        return 0;
    }

    public virtual void Kill()
    {
        currentHealth = 0;
    }

    public void Knockback(Vector3 force)
    { }

    public void Knockback(Vector3 origin, Vector3 force)
    { }

    public float MaxHealth()
    {
        return maxHealth;
    }

    public Vector3 Nameplate()
    {
        return transform.position + Vector3.up * 4;
    }

    public ITarget Parent()
    {
        return this;
    }

    public Vector3 RangedTarget()
    {
        return transform.position;
    }

    public virtual bool RecoilsFrom(ItemMode mode, out float recoilSpeedModifier)
    {
        recoilSpeedModifier = 1;
        return true;
    }

    public Transform Transform()
    {
        return transform;
    }

    public Vector3 Velocity()
    {
        return Vector3.zero;
    }
}
