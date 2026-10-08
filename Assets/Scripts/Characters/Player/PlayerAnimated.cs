using UnityEngine;

public class PlayerAnimated : MonoBehaviour
{
    WeaponObject weapon;

    public void SetWeapon(WeaponObject obj)
    {
        weapon = obj;
    }

    void StartWeaponTrace()
    {
        //weapon.StartWeaponTrace();
    }

    void EndWeaponTrace()
    {
        //weapon.EndWeaponTrace();
    }
}
