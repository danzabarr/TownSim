using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Despawn : MonoBehaviour
{
    public bool start;

    public float life;

    public void StartCountdown(float life)
    {
        this.life = life;
        start = true;
    }

    void Update()
    {
        if (!start)
            return;
        life -= Time.deltaTime;
        if (life <= 0)
            Destroy(gameObject);
    }
}
