using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class DamageableCharacter : NetworkBehaviour, IDamageable
{
    public int Health
    {
        set
        {
            health = value;
            if (value > 0)
            {

            }

            if (health <= 0 && Targetable)
            {
                Targetable = false;
                RemoveCharacter();
            }
        }

        get
        {
            return health;
        }
    }

    public bool Targetable
    {
        get { return targetable; }
        set
        {
            targetable = value;
        }
    }

    [Header("Base Stats")]
    public int maxHealth = 10;
    public int health = 10;
    public bool targetable = true;
    public Rigidbody rb;

    public virtual void OnHit(int damage, Vector3 force)
    {
        Health -= damage;
        rb.AddForce(force, ForceMode.Impulse);
        StartCoroutine(Recover());
        OnHitClientRpc(Health);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public virtual void OnHitClientRpc(int newHealth)
    {
        // LEAVE EMPTY OR IT WILL BREAK GAME IDK WHY
        // Sends data back to clients to update (also allows us to do VFX)
    }

    public virtual IEnumerator Recover()
    {
        rb.freezeRotation = false;
        yield return new WaitForSeconds(2);
        while (Quaternion.Angle(rb.rotation, Quaternion.identity) > 0.1f)
        {
            rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, Quaternion.identity, 0.1f));
        }
        rb.rotation = Quaternion.identity;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        transform.rotation = Quaternion.identity;
    }

    public virtual void RemoveCharacter()
    {
        Destroy(gameObject);
    }
}
