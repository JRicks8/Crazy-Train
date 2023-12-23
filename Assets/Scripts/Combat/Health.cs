using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class Health : MonoBehaviour
{
    public GameObject healthbar;
    public Transform slider;

    public delegate void HealthEventDelegate(GameObject entity);
    public HealthEventDelegate OnDeath;
    public HealthEventDelegate OnDamageTaken;

    [SerializeField]
    private float health = 10;
    [SerializeField]
    private float maxHealth = 10;

    private void Start()
    {
        UpdateHealthBarAppearance();
    }

    public float TakeDamage(float damage)
    {
        health -= damage;
        UpdateHealthBarAppearance();
        OnDamageTaken?.Invoke(gameObject);
        if (health <= 0) OnDeath?.Invoke(gameObject);
        return health;
    }

    public float Heal(float heal)
    {
        health += heal;
        if (health > maxHealth) health = maxHealth;
        UpdateHealthBarAppearance();
        return health;
    }

    public float GetHealth()
    {
        return health;
    }

    public void SetHealth(float newHealth)
    {
        health = newHealth;
        UpdateHealthBarAppearance();
        if (health <= 0) OnDeath?.Invoke(gameObject);
    }

    public void SetCanSeeHealthbar(bool canSee)
    {
        healthbar.SetActive(canSee);
    }

    private void UpdateHealthBarAppearance()
    {
        slider.localScale = new Vector3(health / maxHealth, slider.localScale.y, slider.localScale.z);
    }

    private void OnDestroy()
    {
        OnDeath?.Invoke(gameObject);
    }
}