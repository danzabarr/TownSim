using TownSim.Navigation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TownSim.Items;
using System.Linq;

namespace TownSim.Units
{
    public enum Action
    {
        Idle,
        PickUp,
        Melee,
        Shoot,
        Hit,
        Recoil,
        Mine,
        Chop
    }

    public class Unit : MonoBehaviour, ITarget
    {
        private static List<Unit> units;

        public static IEnumerable Units()
        {
            foreach (Unit unit in units)
                yield return unit;
        }

        public Action CurrentAction { get; protected set; }

        public Vector3 center;
        public Vector3 nameplate;
        public float maxHealth;
        public float currentHealth;
        public int faction;

        public Agent Agent { get; private set; }
        public Rigidbody Rigidbody { get; private set; }
        public Animator Animator { get; private set; }
        public bool strafing;

        [Header("Receive Hit")]
        public float hitSpeed;
        public Transform rangedTargetBone;
        public float knockbackModifier;
        public float hitDamageThreshold;
        public ParticleSystem meleeHitParticles;

        public ITarget Parent()
        {
            return this;
        }

        public Transform Transform()
        {
            return transform;
        }

        public bool RecoilsFrom(ItemMode mode, out float recoilSpeedModifier)
        {
            recoilSpeedModifier = 1;
            return false;
        }

        public Vector3 Center()
        {
            return transform.TransformPoint(center);
        }

        public Vector3 RangedTarget()
        {
            return rangedTargetBone.position;
        }

        public Vector3 Nameplate()
        {
            return transform.TransformPoint(nameplate);
        }

        public Vector3 Velocity()
        {
            return Rigidbody.velocity;
        }

        public float CurrentHealth()
        {
            return currentHealth;
        }

        public float MaxHealth()
        {
            return maxHealth;
        }

        public int Faction()
        {
            return faction;
        }

        protected virtual void Awake()
        {
            Agent = GetComponent<Agent>();
            Rigidbody = GetComponent<Rigidbody>();
            Animator = GetComponent<Animator>();
            Ragdoll rd = GetComponent<Ragdoll>();
            if (rd) rd.Activate(false);

            currentHealth = maxHealth;

            if (units == null)
                units = new List<Unit>();
            units.Add(this);
        }

        protected virtual void OnDestroy()
        {
            if (units != null)
                units.Remove(this);
        }

        public float Damage(float amount)
        {
            if (currentHealth <= 0)
                return 0;
            float difference = currentHealth - Mathf.Clamp(currentHealth - amount, 0, maxHealth);
            currentHealth -= difference;

            if (currentHealth <= 0)
                Kill();

            else if (amount >= hitDamageThreshold)
                Hit();

            return difference;
        }

        public float Damage(float amount, Vector3 knockbackForce)
        {
            if (currentHealth <= 0)
                return 0;
            float difference = currentHealth - Mathf.Clamp(currentHealth - amount, 0, maxHealth);
            currentHealth -= difference;
            Knockback(knockbackForce);
            if (currentHealth <= 0)
            {
                Kill();
                Ragdoll rd = GetComponent<Ragdoll>();
                if (rd) rd.AddForce(knockbackForce * knockbackModifier * .25f, ForceMode.Impulse);
            }
            else if (amount >= hitDamageThreshold)
                Hit();

            return difference;
        }

        public float Damage(float amount, Vector3 knockbackOrigin, Vector3 knockbackForce)
        {
            if (currentHealth <= 0)
                return 0;
            float difference = currentHealth - Mathf.Clamp(currentHealth - amount, 0, maxHealth);
            currentHealth -= difference;
            Knockback(knockbackOrigin, knockbackForce);
            if (currentHealth <= 0)
            {
                Kill();
                Ragdoll rd = GetComponent<Ragdoll>();
                if (rd) rd.AddForceAtPosition(knockbackForce * knockbackModifier * .25f, knockbackOrigin, ForceMode.Impulse);
            }
            if (amount > hitDamageThreshold)
                Hit();

            return difference;
        }

        public void Knockback(Vector3 force)
        {
            Rigidbody.AddForce(force * knockbackModifier, ForceMode.Impulse);
        }

        public void Knockback(Vector3 origin, Vector3 force)
        {
            Rigidbody.AddForceAtPosition(force * knockbackModifier, origin, ForceMode.Impulse);
        }

        public void Hit()
        {
            CurrentAction = Action.Hit;
            Animator.SetTrigger("hit");
            Animator.SetBool("running", false);
        }

        public virtual void Kill()
        {
            Ragdoll rd = GetComponent<Ragdoll>();
            if (rd) rd.Activate(true);
            currentHealth = 0;
        }

        public bool EmitItem(ItemType type, int quantity, out Item item)
        {
            item = null;
            if (type == null || quantity <= 0)
                return false;

            item = Instantiate(type.prefab, transform.TransformPoint(0, 1.5f, 0.5f), transform.rotation * Quaternion.AngleAxis(90, Vector3.up));
            item.quantity = quantity;
            Rigidbody rb = item.GetComponent<Rigidbody>();
            rb.AddForce(transform.forward, ForceMode.Impulse);

            return true;
        }
    }
}
