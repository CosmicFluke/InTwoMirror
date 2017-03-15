using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public float HealthPoints = 100f;
    public Slider HealthBar;

    public float TakeDamage(float amount)
    {
        HealthPoints -= amount;
        UpdateHealthBar();

        return HealthPoints;
    }

    public float Kill()
    {
        return TakeDamage(HealthPoints);
    }

    private void UpdateHealthBar()
    {
        HealthBar.value = HealthPoints / 100;
    }
}
