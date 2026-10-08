using Unity.Netcode;
using UnityEngine;

public class SpawnWeapon : NetworkBehaviour
{
    public WeaponObject weapon;

    public virtual void EquipWeapon()
    {
        Debug.Log("spawned weapon");
        // Get weapon from damageable script
        // Spawn it
        // Initialize stats
    }

    public void StartWeaponTrace()
    {
        Debug.Log("start");
        weapon.StartWeaponTrace();
    }

    public void EndWeaponTrace()
    {
        Debug.Log("end");
        weapon.EndWeaponTrace();
    }
}
