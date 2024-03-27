using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class Health : MonoBehaviour
{
    [SerializeField] private GameObject healthbar;
    [SerializeField] private Transform scrollerContainer;

    [SerializeField] private float health = 10f;
    [SerializeField] private float maxHealth = 10f;
    [SerializeField] private bool isPlayer = false;
    [SerializeField] private bool isInvincible = false;

    public delegate void HealthEventDelegate(GameObject entity);
    public HealthEventDelegate OnDeath;
    public HealthEventDelegate OnDamageTaken;
    public delegate void HealthChangeEventDelegate(float newHealth, float newMaxHealth);
    public HealthChangeEventDelegate OnHealthChanged;

    private void Start()
    {
        if (TryGetComponent(out PlayerController _))
            isPlayer = true;
        else 
            UpdateHealthBarAppearance();

        OnHealthChanged?.Invoke(health, maxHealth);
    }

    public void TakeDamage(float damage)
    {
        if (isInvincible) return;
        if (isPlayer) 
        {
            damage = Mathf.Floor(damage);
        }
        if (health <= 0)
        {
            health = 0;
            OnHealthChanged?.Invoke(0, maxHealth);
            return;
        }
        health -= damage;
        OnDamageTaken?.Invoke(gameObject);
        OnHealthChanged?.Invoke(Mathf.Max(0, health), maxHealth);
        if (health <= 0)
        {
            health = 0;
            OnDeath?.Invoke(gameObject);
        }
        if (!isPlayer) UpdateHealthBarAppearance();
        return;
    }

    public float Heal(float heal)
    {
        if (isPlayer)
            heal = Mathf.Floor(heal);

        if (health <= 0)
        {
            health = 0;
            OnHealthChanged?.Invoke(0, maxHealth);
            return 0;
        }
        health += heal;
        if (health > maxHealth) health = maxHealth;
        OnHealthChanged?.Invoke(health, maxHealth);
        if (!isPlayer) UpdateHealthBarAppearance();
        return health;
    }

    public float GetHealth()
    {
        return health;
    }

    public bool GetIsDead()
    {
        return health <= 0;
    }

    public void SetHealth(float newHealth)
    {
        health = newHealth;
        if (!isPlayer) UpdateHealthBarAppearance();
        if (health <= 0)
        {
            health = 0;
            OnDeath?.Invoke(gameObject);
        }
        OnHealthChanged?.Invoke(health, maxHealth);
    }

    public void SetMaxHealth(int maxHealth, bool adjustCurrentHealth)
    {
        this.maxHealth = maxHealth;
        if (adjustCurrentHealth)
            health = maxHealth;
        OnHealthChanged?.Invoke(health, maxHealth);
    }

    public void SetCanSeeHealthbar(bool canSee)
    {
        healthbar.SetActive(canSee);
    }

    public void SetIsInvincible(bool isInvincible)
    {
        this.isInvincible = isInvincible;
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