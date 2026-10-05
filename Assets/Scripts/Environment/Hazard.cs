using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;

public class Hazard : NetworkBehaviour
{
    public int damage = 1;
    public float knockbackForce = 25f;

    List<DamageableCharacter> hitCharacters = new List<DamageableCharacter>();

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer)
        {
            return;
        }

        DamageableCharacter character = collision.transform.root.GetComponent<DamageableCharacter>();
        if (character && collision.contactCount > 0 && !hitCharacters.Contains(character))
        {
            hitCharacters.Add(character);
            ContactPoint point = collision.GetContact(0);
            Vector3 contactPoint = point.point;
            Vector3 direction = (character.transform.position + Vector3.up * 0.5f - contactPoint).normalized;
            character.OnHit(damage, direction * knockbackForce);
            StartCoroutine(RemoveCharacter(character));
        }
    }

    IEnumerator RemoveCharacter(DamageableCharacter character)
    {
        yield return new WaitForSeconds(.5f);
        if (hitCharacters.Contains(character))
        {
            hitCharacters.Remove(character);
        }
    }
}
