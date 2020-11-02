using TownSim.Items;
using UnityEngine;

public interface ITarget 
{
    ITarget Parent();
    Transform Transform();
    Vector3 Center();
    Vector3 Nameplate();
    Vector3 RangedTarget();
    Vector3 Velocity();
    bool RecoilsFrom(ItemMode item, out float recoilSpeedModifier);
    float Damage(float amount);
    float Damage(float amount, Vector3 knockbackForce);
    float Damage(float amount, Vector3 knockbackOrigin, Vector3 knockbackForce);
    void Knockback(Vector3 force);
    void Knockback(Vector3 origin, Vector3 force);
    void Kill();
    float CurrentHealth();
    float MaxHealth();
    int Faction();
}
