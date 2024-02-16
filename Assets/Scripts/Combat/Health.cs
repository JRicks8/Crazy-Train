using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class Health : MonoBehaviour
{
    [SerializeField] private GameObject healthbar;
    [SerializeField] private Transform scrollerContainer;

    [SerializeField] private float health = 10f;
    [SerializeField] private float maxHealth = 10f;

    public delegate void HealthEventDelegate(GameObject entity);
    public HealthEventDelegate OnDeath;
    public HealthEventDelegate OnDamageTaken;

    private void Start()
    {
        UpdateHealthBarAppearance();
    }

    public float TakeDamage(float damage)
    {
        if (health <= 0)
        {
            health = 0;
            return 0;
        }
        health -= damage;
        UpdateHealthBarAppearance();
        OnDamageTaken?.Invoke(gameObject);
        if (health <= 0)
        {
            health = 0;
            OnDeath?.Invoke(gameObject);
        }
        return health;
    }

    public float Heal(float heal)
    {
        if (health <= 0)
        {
            health = 0;
            return 0;
        }
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
        if (health <= 0)
        {
            health = 0;
            OnDeath?.Invoke(gameObject);
        }
    }

    public void SetMaxHealth(int maxHealth, bool adjustCurrentHealth)
    {
        this.maxHealth = maxHealth;
        if (adjustCurrentHealth)
            health = maxHealth;
    }

    public void SetCanSeeHealthbar(bool canSee)
    {
        healthbar.SetActive(canSee);
    }

    private void UpdateHealthBarAppearance()
    {
        if (scrollerContainer == null)
        {
            Debug.LogError("The scrollerContainer is null, cannot update healthbar appearance!");
            return;
        }

        scrollerContainer.localScale = new Vector3(Mathf.Clamp01(health / maxHealth), scrollerContainer.localScale.y, 1);
    }

    private void OnDestroy()
    {
        if (health <= 0) return; // if already dead don't invoke the ondeath delegate
        OnDeath?.Invoke(gameObject);
    }
}