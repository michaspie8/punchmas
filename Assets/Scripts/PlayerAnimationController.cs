using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerAnimationController : FighterAnimationController
{

    private void Start()
    {

        anim = GetComponent<Animator>();
        fighter = PlayerController.instance;
    }
    private void Update()
    {

    }
}
