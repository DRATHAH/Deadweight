using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "DEADWEIGHT/Weapon")]
public class Weapon : ScriptableObject
{
    new public string name = "New Weapon";
    public GameObject prefab;
    public int dmg = 1;
    [Tooltip("Lower numbers = faster swing speed")]
    public float swingSpeed = 1;
    [Tooltip("Lower numbers = faster recovery speed")]
    public float attackDelay = 1;
    public float knockbackStrength = 10;
}
