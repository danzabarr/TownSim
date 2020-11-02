using TownSim.Items;
using UnityEngine;

namespace TownSim.Units
{
    public class UnitChild : MonoBehaviour, ITarget
    {
        private Unit parent;

        public float damageModifier;
        public float knockbackModifier;
        public bool recoil;
        public float recoilSpeedModifier = 1;

        private void Awake()
        {
            parent = GetComponentInParent<Unit>();
        }

        public ITarget Parent()
        {
            return parent;
        }

        public bool RecoilsFrom(ItemMode mode, out float recoilSpeedModifier)
        {
            recoilSpeedModifier = this.recoilSpeedModifier;
            return recoil;
        }

        public Vector3 Center()
        {
            return parent.Center();
        }

        public Vector3 RangedTarget()
        {
            return parent.RangedTarget();
        }

        public float CurrentHealth()
        {
            return parent.CurrentHealth();
        }

        public float Damage(float amount)
        {
            return parent.Damage(amount * damageModifier);
        }

        public float Damage(float amount, Vector3 knockbackForce)
        {
            return parent.Damage(amount * damageModifier, knockbackForce * knockbackModifier);
        }

        public float Damage(float amount, Vector3 knockbackOrigin, Vector3 knockbackForce)
        {
            return parent.Damage(amount * damageModifier, knockbackOrigin, knockbackForce * knockbackModifier);
        }

        public int Faction()
        {
            return parent.Faction();
        }

        public void Kill()
        {
            parent.Kill();
        }

        public void Knockback(Vector3 force)
        {
            parent.Knockback(force * knockbackModifier);
        }

        public void Knockback(Vector3 origin, Vector3 force)
        {
            parent.Knockback(origin, force * knockbackModifier);
        }

        public float MaxHealth()
        {
            return parent.MaxHealth();
        }

        public Vector3 Nameplate()
        {
            return parent.Nameplate();
        }

        public Transform Transform()
        {
            return parent.Transform();
        }

        public Vector3 Velocity()
        {
            return parent.Velocity();
        }
    }
}
