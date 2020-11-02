using System.Collections;
using System.Collections.Generic;
using TownSim.Items;
using UnityEngine;

public class Rocks : ResourceNode
{
    public ParticleSystem onHit;

    public int particleEmission;
    public float itemVelocity;

    [Range(0, 1)] public float itemDropChance;
    public AnimationCurve itemDropNumber;

    public void Mine(float dropChanceModifier, Vector3 collision)
    {
        if (currentHealth <= 0)
            return;
        onHit.transform.position = collision;
        onHit.Emit(particleEmission);

        //if (Random.value > itemDropChance * dropChanceModifier)
        //    return;

        Damage(1);

        if (currentHealth <= 0)
            Deplete();

    }

    public void Deplete()
    {
        ItemType rock = ItemManager.Type("Rock");

        int amount = (int)itemDropNumber.Evaluate(Random.value);
        Debug.Log("Dropped " + amount);
        for (int i = 0; i < amount; i++)
        {
            Vector3 position = transform.position + Random.insideUnitSphere;
            float mesh = Map.Instance.MeshHeight(position.xz());
            position.y = Mathf.Max(position.y, mesh + 1);

            Item item = Instantiate(rock.prefab, position, Quaternion.LookRotation(Random.onUnitSphere));
            item.quantity = 1;
        }
        Destroy(gameObject);
    }
}
