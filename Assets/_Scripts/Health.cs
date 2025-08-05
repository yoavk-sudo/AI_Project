using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] int health = 100;
    [SerializeField, Min(0)] int numberOfHeals = 3;

    public int HP { get => health; }
    public int NumberOfHeals { get => numberOfHeals; }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Potion"))
        {
            if(Vector3.Distance(transform.position, other.transform.position) > 2f)
            {
                return;
            }
            Debug.Log($"<color=green>{gameObject.name} has picked up a potion!</color>");
            GainPotion();
            Destroy(other.gameObject);
        }
    }

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

    private void GainPotion()
    {
        numberOfHeals++;
        Debug.Log($"<color=green>Gained a potion! {gameObject.name} now has {numberOfHeals} heals left.</color>");
    }
}
