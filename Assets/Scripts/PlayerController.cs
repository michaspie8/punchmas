using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : Fighter
{
    public static PlayerController instance;
    private void Awake()
    {

        if (instance != null)
        {
            Debug.LogWarning("More than one instance of PlayerController found!");
            return;
        }
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        Setup();
        anim = GetComponent<Animator>();
        animController = GetComponent<PlayerAnimationController>();
        //find the enemy script and subscribe to the onHit event
        

        var enemy = GameObject.FindGameObjectsWithTag("Enemy").FirstOrDefault();
        if (enemy != null)
        {
            var enemyScript = enemy.GetComponent<Enemy>();
            if (enemyScript != null)
            {
                onHit += enemyScript.HitResponse;
                if(animController != null)
                animController.dodge += enemyScript.DodgeResponse;
                    
            }
        }

        
    }

    // Update is called once per frame
    void Update()
    {
    }

    public override void Die()
    {
        base.Die();
        StartCoroutine(UIManager.instance.FightEndCutscene(null));
    }

}


