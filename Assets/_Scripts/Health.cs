using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] int health = 100;
    [SerializeField, Min(1)] int numberOfHeals = 3;

    public int HP { get => health; }
    public int NumberOfHeals { get => numberOfHeals; }

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

    public bool Heal(int amount)
    {
        if(numberOfHeals <= 0)
        {
            Debug.Log($"<color=orange>No heals left for {gameObject.name}</color>");
            return false;
        }
        numberOfHeals--;
        health += amount;
        return true;
    }
}
