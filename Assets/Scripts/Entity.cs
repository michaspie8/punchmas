using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public string Name;
    public float initialHealth;
    public float maxHealth = -1;
    [SerializeField]
    private float health;
    public float HealthValue { get { return health; } }

    public void IncreaseHealth(float value)
    {
        if (maxHealth != -1 && health + value > maxHealth)
        {
            health = maxHealth;
        }
        else
            health += value;
    }
    public void DecreaseHealth(float value)
    {
        if (health - value < 0)
        {
            health = 0;
            Die();
        }
        else
            health -= value;
    }
    public virtual void Die()
    {
        Debug.Log("Entity " + this.gameObject.name + " died.");
    }

    public virtual void Setup()
    {
        if (name == null)
        {
            name = gameObject.name;
        }
        health = initialHealth;
    }

    private void Start()
    {
        Setup();
    }
}



