using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public abstract class Fighter : Entity
{
    public FighterState fighterState = FighterState.Idle;
    public Coroutine KnockCo = null;
    public Animator anim;
    public FighterAnimationController animController;
    public int damage = 10;
    public delegate void OnHitDelegate(string typeOfAttack);
    public OnHitDelegate onHit;
    public Dictionary<string, float> damageMultiplers = new() {
        { "lead-jab", 0.5f },
        { "cross-jab", 0.6f },
        { "hook", 1f },
        { "kick", 2f },
    };
    public bool hadDodged = false;
    public bool died = false;
    public int points = 0;

    // Start is called before the first frame update
    void Start()
    {
        Setup();
    }

    // Update is called once per frame
    void Update()
    {

    }

    //Animation event 
    public void OnHit(string typeOfAttack)
    {
        onHit.Invoke(typeOfAttack);
    }

    public void DodgeResponse(bool isDodging)
    {
        hadDodged = isDodging;
    }

    public void HitResponse(string typeOfAttack)
    {

        //Check if plyer is dodging or guarding 
        switch (fighterState)
        {
            case FighterState.Dodgeing:
                Debug.Log(name + "dodged the attack");
                break;
            case FighterState.Stunned:
                Debug.Log(name + "Player is stunned");
                break;
            default:
                if (hadDodged)
                {
                    Debug.Log(name + "dodged the attack");
                }
                else
                {
                    Debug.Log(name + " got hit by " + typeOfAttack);
                    var dam = damage;
                    if (damageMultiplers.ContainsKey(typeOfAttack))
                    {
                        DecreaseHealth(damage * damageMultiplers[typeOfAttack]);
                        points += (int)(damage * (damageMultiplers[typeOfAttack] * damageMultiplers[typeOfAttack]) * 10);
                    }
                    else
                    {
                        DecreaseHealth(damage);
                    }
                    StartCoroutine(animController.KnockCo());
                }
                break;
        }
    }

    public void OnActionButton(int action)
    {
        switch (action)
        {
            case 0:
                Debug.Log("Universal button pressed");
                if (fighterState == FighterState.Idle && anim.GetCurrentAnimatorStateInfo(0).IsName("Idle") && !anim.IsInTransition(0))
                {
                    StartCoroutine(animController.HookCo());
                }
                break;
            case 1:
                Debug.Log("First action button pressed");
                //simple punch
                if (fighterState == FighterState.Idle && anim.GetCurrentAnimatorStateInfo(0).IsName("Idle") && !anim.IsInTransition(0))
                {
                    StartCoroutine(animController.JabCo());
                }
                break;
            case 2:
                Debug.Log("Second action button pressed");
                if (fighterState == FighterState.Idle && anim.GetCurrentAnimatorStateInfo(0).IsName("Idle") && !anim.IsInTransition(0))
                {
                    StartCoroutine(animController.KickCo());
                }
                break;
            case 3:
                Debug.Log("Third action button pressed");
                if (fighterState == FighterState.Idle && anim.GetCurrentAnimatorStateInfo(0).IsName("Idle") && !anim.IsInTransition(0))
                {
                    StartCoroutine(animController.DodgeCo());
                }
                break;
            default:
                Debug.Log("Wrong action button pressed");
                break;
        }
    }
    public override void Die()
    {
        if (died == false)
        {
            base.Die();
            died = true;
            anim.SetTrigger("die");
        }
    }
}

public enum FighterState { Idle, Punching, Kicking, Stunned, Dodgeing, Dead };