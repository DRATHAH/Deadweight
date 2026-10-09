using UnityEngine;

public class PlayerAnimated : MonoBehaviour
{
    WeaponObject weapon;

    public void SetWeapon(WeaponObject obj)
    {
        weapon = obj;
    }

    // Called in animation
    void StartWeaponTrace()
    {
        weapon.StartWeaponTrace();
    }

    // Called in animation
    void EndWeaponTrace()
    {
        weapon.EndWeaponTrace();
    }
}
