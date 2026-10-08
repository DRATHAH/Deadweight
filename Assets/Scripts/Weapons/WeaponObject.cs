using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class WeaponObject : MonoBehaviour
{
    public int damage = 1;
    public float knockback = 10f;
    public CapsuleCollider collider;

    DamageableCharacter owner;
    List<DamageableCharacter> hitCharacters = new List<DamageableCharacter>();

    private void Start()
    {
        owner = transform.root.GetComponent<DamageableCharacter>();
        collider.enabled = false;
    }

    public void StartWeaponTrace()
    {
        collider.enabled = true;
    }

    public void EndWeaponTrace()
    {
        collider.enabled = false;
        hitCharacters.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("hit");
        DamageableCharacter character = other.transform.root.GetComponent<DamageableCharacter>();
        if (character && character != owner && !hitCharacters.Contains(character))
        {
            hitCharacters.Add(character);
            Vector3 contactPoint = transform.position;
            Vector3 direction = (character.transform.position + Vector3.up * 0.5f - contactPoint).normalized;
            character.OnHit(damage, direction * knockback);
        }
    }
}
