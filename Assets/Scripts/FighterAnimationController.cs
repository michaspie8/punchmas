using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class FighterAnimationController : MonoBehaviour
{

    public Animator anim;
    public Fighter fighter;
    public delegate void Dodge(bool isGuarding);
    public Dodge dodge;
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        fighter = GetComponent<Fighter>();

    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public bool nextJab = false;
    private bool wasNextJabPressed = false;
    [SerializeField]
    bool leadJab = false;
    [SerializeField]
    bool crossJab = false;
    

    


    public void handleNextJabPress()
    {
        if (nextJab)
        {
            anim.SetTrigger("NextJab");
            wasNextJabPressed = true;
        }
    }



    public IEnumerator JabCo()
    {
        bool end = false;
        StopCoroutine(JabCo());
        //anim.ResetTrigger("NextJab");
        if (fighter.fighterState != FighterState.Idle)
        {
            yield break;
        }
        anim.SetTrigger("jab");
        fighter.fighterState = FighterState.Punching;
        while (!end)
        {

            //wait until the puch animation starts
            yield return new WaitForEndOfFrame();
            yield return new WaitUntil(() => leadJab || crossJab || (anim.GetCurrentAnimatorStateInfo(0).IsName("Idle") && !anim.IsInTransition(0)));
            if (!leadJab && !crossJab)
            {

                /*yield return new WaitUntil(() => anim.IsInTransition(0));
                yield return new WaitUntil(() => leadJab || crossJab || );
                if (!leadJab && !crossJab)
                {*/
                    end = true; break;
                //}
            }
            var punchType = "";
            nextJab = true;
            if (leadJab)
            {
                punchType = "lead-jab";
                yield return new WaitUntil(() => !leadJab);
            }
            else if (crossJab)
            {
                punchType = "cross-jab";
                yield return new WaitUntil(() => !crossJab);
            }
            else
            {
                nextJab = false;
                //anim.ResetTrigger("NextJab");
                end = true;
                break;
            }
            nextJab = false;
            if (!wasNextJabPressed)
            {
                end = true;

            }
            wasNextJabPressed = false;
            yield return new WaitForEndOfFrame();

        }


        fighter.fighterState = FighterState.Idle;
        //anim.ResetTrigger("NextJab");
        yield return null;
    }

    public IEnumerator HookCo()
    {
        StopCoroutine(HookCo());
        anim.SetTrigger("hook");
        fighter.fighterState = FighterState.Punching;
        yield return new WaitUntil(() => (anim.GetCurrentAnimatorStateInfo(0).IsName("Idle") && !anim.IsInTransition(0)));
        fighter.fighterState = FighterState.Idle;
        yield return null;
    } 
    public IEnumerator KickCo()
    {
        StopCoroutine(HookCo());
        anim.SetTrigger("kick");
        fighter.fighterState = FighterState.Punching;
        yield return new WaitUntil(() => (anim.GetCurrentAnimatorStateInfo(0).IsName("Idle") && !anim.IsInTransition(0)));
        fighter.fighterState = FighterState.Idle;
        yield return null;
    }

    public IEnumerator DodgeCo()
    {
        StopCoroutine(DodgeCo());
        fighter.fighterState = FighterState.Dodgeing;
        dodge.Invoke(true);
        anim.SetTrigger("dodge");
        yield return new WaitUntil(()=>anim.IsInTransition(0));
        yield return new WaitUntil(() => (anim.GetCurrentAnimatorStateInfo(0).IsName("Idle") && !anim.IsInTransition(0)));
        yield return new WaitForEndOfFrame();
        fighter.fighterState = FighterState.Idle;
        dodge.Invoke(false);
        yield return null;
    }

    public void startAnim(string animName)
    {
        switch (animName)
        {
            case "lead-jab":
                leadJab = true;
                break;
            case "cross-jab":
                crossJab = true;
                break;
            case "dodge":
                //todo check if the player is already dodging with a bool
                break;
            default:
                break;
        }
    }

    public void endAnim(string animName)
    {
        switch (animName)
        {
            case "lead-jab":
                leadJab = false;
                break;
            case "cross-jab":
                crossJab = false;
                break;

            default:
                break;
        }

    }


    public IEnumerator KnockCo()
    {
        StopCoroutine(KnockCo());
        if (fighter.fighterState != FighterState.Idle)
        {
            yield break;
        }
        //When the player got punched, he gets some knockback
        anim.SetTrigger("knock");
        fighter.fighterState = FighterState.Stunned;
        //if player was attacking, stop the attack
        if (fighter.fighterState == FighterState.Punching)
        {
            StopCoroutine(JabCo());
            StopCoroutine(HookCo());
        }


        yield return new WaitUntil(() => (anim.GetCurrentAnimatorStateInfo(0).IsName("Idle") && !anim.IsInTransition(0)));
        fighter.fighterState = FighterState.Idle;
        yield return null;

    }
}
