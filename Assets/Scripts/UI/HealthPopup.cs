using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HealthPopup : NetworkBehaviour
{
    public GameObject healthPopup;
    public Image healthBar;

    private void Start()
    {
        healthPopup.SetActive(false);
    }

    public void TriggerPopup(int health, int maxHealth)
    {
        StartCoroutine(TriggerHealthPopup(health, maxHealth));
    }

    IEnumerator TriggerHealthPopup(int health, int maxHealth)
    {
        healthPopup.SetActive(true);
        healthBar.fillAmount = (float)health / (float)maxHealth;
        yield return new WaitForSeconds(2);
        healthPopup.SetActive(false);
    }

    private void Update()
    {
        healthPopup.transform.position = Vector3.Slerp(healthPopup.transform.position, Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2.5f), 0.1f);
    }
}
