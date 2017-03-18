using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Player))]
public class PlayerHealth : MonoBehaviour
{
    public float healthpointsReadOnly;

    [Range(0, 100)]
    public float healingRate = 10f; // points per second

    private float initialHealthPoints;
    private PlayerID player;
    private bool dying = false;
    private Slider healthBar;

    public float HealthPoints {
        get {
            return healthpointsReadOnly;
        }
        set {
            healthpointsReadOnly = value;
            if (healthBar != null)
                UpdateHealthBar();
        }
    }

    void Start()
    {
        player = GetComponent<Player>().player;
        // TODO: modify to get sliders from LevelController, once LevelController is modified to instantiate and/or find these
        Slider[] sliders = GameObject.FindGameObjectWithTag("MainHUD").GetComponentsInChildren<Slider>();
        if (sliders.Length == 0) throw new System.Exception("Could not find Health Bar sliders");
        healthBar = sliders
            .Where(s => s.name.Contains(player.ToString()))
            .First();
        HealthPoints = initialHealthPoints;
    }

    /// <summary>Should only be called by Player component.</summary>
    public void ApplyDamage(float amount)
    {
        if (amount < 0 && amount + HealthPoints > initialHealthPoints)
            amount = -(initialHealthPoints - HealthPoints);
        HealthPoints -= amount;
        if (HealthPoints <= 0) GetComponent<Player>().Kill();
    }

    private void UpdateHealthBar()
    {
        healthBar.value = HealthPoints / 100;
    }

    // Checks distance between this and other player
    // If on adjacent regions and both players make noise, will heal at healingRate
    // Call this when both players are making noise.
    public void CoopHeal()
    {
        // Coop healing (not working yet)
        //PlayerMovementController movementController = GetComponent<PlayerMovementController>();
        //if (movementController.Region != null &&
        //    movementController.otherPlayer != null &&
        //    movementController.Region.Neighbours.Any(obj => obj.GetComponent<Region>().IsOccupied))
        //{
        //    movementController.otherPlayer.GetComponent<PlayerHealth>().ApplyDamage(-healingRate * Time.deltaTime);
        //}
    }
}
