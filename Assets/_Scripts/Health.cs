using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] int health = 100;
    [SerializeField] int maxHealth = 100;
    [SerializeField, Min(0)] int numberOfHeals = 3;

    public int HP { get => health; }
    public int NumberOfHeals { get => numberOfHeals; }
    public Action OnHit;
    public Action OnDeath;
    public Action OnHeal;
    private void Awake()
    {
        maxHealth = health;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Potion"))
        {
            if(Vector3.Distance(transform.position, other.transform.position) > 2f)
            {
                return;
            }
            //Debug.Log($"<color=green>{gameObject.name} has picked up a potion!</color>");
            GainPotion();
            Destroy(other.gameObject);
        }
    }

    public void TakeDamage(int damage, Action hasSenderGotAKill)
    {
        health -= damage;
        if (health <= 0)
        {
            Die(hasSenderGotAKill);
        }
    }

    private void Die(Action hasSenderGotAKill)
    {
        hasSenderGotAKill?.Invoke();
        gameObject.SetActive(false);//destroy when training session end.
    }

    public bool Heal(int amount)
    {
        if(numberOfHeals <= 0)
        {
            //Debug.Log($"<color=orange>No heals left for {gameObject.name}</color>");
            return false;
        }
        numberOfHeals--;
        health += amount;
        return true;
    }

    private void GainPotion()
    {
        numberOfHeals++;
        //Debug.Log($"<color=green>Gained a potion! {gameObject.name} now has {numberOfHeals} heals left.</color>");
    }
    public int GetHealth()
    {
        return health;
    }
    public int GetMaxHealth()
    {
        return maxHealth;
    }
}
