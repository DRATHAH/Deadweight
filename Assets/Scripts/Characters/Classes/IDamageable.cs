using UnityEngine;

public interface IDamageable
{
    int Health { set; get; }
    bool Targetable { get; set; }

    void OnHit(int damage, Vector3 force);
    void RemoveCharacter();
}
