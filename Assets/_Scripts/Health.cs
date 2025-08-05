using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] int health = 100;

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    public void Heal(int amount)
    {
        health += amount;
    }
}
