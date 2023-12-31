using Cinemachine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerController : Fighter
{
    public static PlayerController instance;
    public GameObject wireframeParent;
    public Transform cameraLookAtTransform;


    private void Awake()
    {

        if (instance != null)
        {
            Debug.LogWarning("More than one instance of PlayerController found!");
            return;
        }
        instance = this;
        foreach (var obj in wireframeParent.GetComponentsInChildren<Transform>())
        {
            var r = obj.GetComponent<WireframeRenderer>();
            if (r != null) r.LineColor = Color.red;
        }
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
                if (animController != null)
                    animController.dodge += enemyScript.DodgeResponse;

            }
        }


        if (cameraLookAtTransform != null)
        {
            var a = FindObjectsByType<CinemachineVirtualCamera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var cam in a)
            {
                if (cam.LookAt == null)
                    cam.LookAt = cameraLookAtTransform;
                if (cam.Follow == null)
                    cam.Follow = transform;
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


