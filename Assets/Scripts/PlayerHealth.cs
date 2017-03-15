using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public float HealthPoints = 100f;
    public Slider HealthBar;
    [Range(0, 10)] public float RespawnDelay = 1f; // TODO: replace with death animation time
    private float initialHealthPoints;

    private bool dying = false;

    void Start()
    {
        UpdateHealthBar();
        initialHealthPoints = HealthPoints;
    }

    public void ApplyDamage(float amount)
    {
        HealthPoints -= amount;
        if (HealthPoints <= 0) Die();

        UpdateHealthBar();
    }

    public void Die()
    {
        // Death lock
        if (!dying) dying = true;
        else return;

        Debug.Log(name + " has died.");
        HealthPoints = 0;
        AnimatedCharacter characterAnimation = GetComponentInChildren<AnimatedCharacter>();
        if (characterAnimation != null)
            characterAnimation.SetAnimation("Die");
        StartCoroutine(death());
    }

    IEnumerator death()
    {
        yield return new WaitForSeconds(RespawnDelay);
        gameObject.SetActive(false);
        GetComponent<PlayerMovementController>().Spawn();
        HealthPoints = initialHealthPoints;
        UpdateHealthBar();
        gameObject.SetActive(true);
        dying = false; // Death unlock
    }

    private void UpdateHealthBar()
    {
        HealthBar.value = HealthPoints / 100;
    }
}
