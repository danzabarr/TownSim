using System.Collections;
using System.Collections.Generic;
using TownSim.Animation;
using TownSim.Items;
using UnityEngine;

public class Tree : ResourceNode
{
    public TreeStump stump;
    public Rigidbody trunk;
    public Renderer trunkRenderer;
    public Renderer branchesRenderer;
    private MaterialPropertyBlock properties;
    private bool startedFalling;

    public AnimationCurve shake;
    public float shakeDuration;
    public float fallDuration;
    public ParticleSystem onHit;
    public int particleEmission;

    public bool Fallen { get; private set; }

    private void Awake()
    {
        trunk.centerOfMass = new Vector3(0, 3, 0);
    }

    public void SetColor(Color color)
    {
        branchesRenderer.material.color = color;
    }

    public void SetSize(float size)
    {
        transform.localScale = Vector3.one * size;
    }

    public void Chop(float amount, Vector3 position, Vector3 direction)
    {
        if (CurrentHealth() <= 0)
            return;

        Damage(amount);

        onHit.transform.position = position;
        onHit.Emit(particleEmission);

        if (CurrentHealth() <= 0)
            Fell(direction);
        else
            StartCoroutine(Shake(direction));
    }
    
    private IEnumerator Shake(Vector3 direction)
    {
        direction.y = 0;

        Vector3 axis = Vector3.Cross(Vector3.up, direction);
        for (float t = 0; t < shakeDuration; t += Time.deltaTime)
        {
            float e = shake.Evaluate(t / shakeDuration);
            trunk.transform.rotation *= Quaternion.AngleAxis(e, axis);
            yield return null;
        }

        for (float t = 0; t < Quaternion.Angle(transform.transform.localRotation, Quaternion.identity); t += Time.deltaTime)
        {
            if (!startedFalling)
                trunk.transform.localRotation = Quaternion.Lerp(trunk.transform.localRotation, Quaternion.identity, Time.deltaTime);
            yield return null;
        }
    }

    public override void Kill()
    {
        Fell(Random.insideUnitCircle.x0y());
    }

    public void Fell(Vector3 force)
    {
        if (startedFalling)
            return;

        StopAllCoroutines();

        startedFalling = true;
        trunk.isKinematic = false;
        properties = new MaterialPropertyBlock();

        stump.gameObject.AddComponent<Despawn>().StartCountdown(20);
        StartCoroutine(StopWind());

        trunk.AddForceAtPosition(force, trunk.transform.position + Vector3.up * 8, ForceMode.VelocityChange);
        Debug.Log("TIMBERRR!");
    }

    private IEnumerator StopWind()
    {
        float initialStrength = trunkRenderer.sharedMaterial.GetFloat("_WindStrength");

        for (float t = 0; t < fallDuration; t += Time.deltaTime)
        {
            properties.SetFloat("_WindStrength", (1 - t / fallDuration) * initialStrength);
            trunkRenderer.SetPropertyBlock(properties);
            branchesRenderer.SetPropertyBlock(properties);
            yield return null;
        }
        properties.SetFloat("_WindStrength", 0);
        trunkRenderer.SetPropertyBlock(properties);
        branchesRenderer.SetPropertyBlock(properties);

        Fallen = true;
        Debug.Log("YO");

        int logs = 3;
        float height = 12;

        for (int i = 0; i < logs; i++)
        {
            Item item = ItemManager.Item("Log", 1);
            item.transform.position = trunk.transform.TransformPoint(new Vector3(0, height * (i + 1f) / (logs + 1f), 0));
            item.transform.forward = trunk.transform.up;
        }

        Destroy(trunk.gameObject);
    }
}
