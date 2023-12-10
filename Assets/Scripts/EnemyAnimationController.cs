using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimationController : FighterAnimationController
{
    // Start is called before the first frame update
    void Start()
    {
        fighter = GetComponent<Enemy>();
        anim = GetComponent<Animator>();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
