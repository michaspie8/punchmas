using NUnit.Framework.Constraints;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy : Fighter
{
    bool AIActionInProgress = false;
    bool attackEnabled = true;
    
    
    // Start is called before the first frame update
    void Start()
    {
        Setup();
        anim = GetComponent<Animator>();
        animController = GetComponent<EnemyAnimationController>();
        onHit += PlayerController.instance.HitResponse;
        if (animController != null)
            animController.dodge += PlayerController.instance.DodgeResponse;

    }

    // Update is called once per frame
    void Update()
    {
        if (AIActionInProgress == false && attackEnabled)
        {

            switch (Random.Range(1, 5))
            {
                case 1:
                    StartCoroutine(AiAttack1());
                    break;
                case 2:
                    StartCoroutine(AiAttack2());
                    break;
                default:
                    break;
            }
        }
    }

    public virtual IEnumerator AiAttack1()
    {
        AIActionInProgress = true;
        yield return new WaitForSecondsRealtime(0.3f);
        OnActionButton(1);
        yield return new WaitForSecondsRealtime(0.3f);
        OnActionButton(1);
        yield return new WaitForSecondsRealtime(0.3f);
        OnActionButton(1);
        yield return new WaitForSecondsRealtime(0.3f);
        OnActionButton(1);
        yield return new WaitForSecondsRealtime(0.5f);
        OnActionButton(3);
        yield return new WaitForSecondsRealtime(1);
        AIActionInProgress = false;
        yield break;
    }

    public virtual IEnumerator AiAttack2()
    {
        AIActionInProgress = true;
        yield return new WaitForSecondsRealtime(0.3f);
        OnActionButton(2);
        yield return new WaitForSecondsRealtime(0.7f);
        OnActionButton(1);
        yield return new WaitForSecondsRealtime(0.3f);
        OnActionButton(1);
        yield return new WaitForSecondsRealtime(0.3f);
        OnActionButton(3);
        yield return new WaitForSecondsRealtime(0.5f);
        OnActionButton(3);
        yield return new WaitForSecondsRealtime(1);
        AIActionInProgress = false;
        yield break;
    }

    public override void Die()
    {
        if (died == false)
        {
            base.Die();
            attackEnabled = false;
            StopCoroutine(AiAttack1());
            StopCoroutine(AiAttack2());
            StartCoroutine(UIManager.instance.FightEndCutscene(this));
        }
    }
}
