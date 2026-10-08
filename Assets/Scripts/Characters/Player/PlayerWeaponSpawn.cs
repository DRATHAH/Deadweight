using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerWeaponSpawn : SpawnWeapon
{
    public PlayerAnimated animatedPlayer;

    Player player;

    public override void OnNetworkSpawn()
    {
        player = GetComponent<Player>();

        Weapon playerWeapon = GetComponent<Player>().equippedWeapon;
        if (playerWeapon != null)
        {
            EquipWeapon();
        }
    }

    public override void EquipWeapon()
    {
        GameObject weaponObj = Instantiate(player.equippedWeapon.prefab, player.weaponPoint.position, player.weaponPoint.rotation);
        weaponObj.transform.parent = player.weaponPoint;
        weapon = weaponObj.GetComponent<WeaponObject>();
        animatedPlayer.SetWeapon(weapon);
    }
}
