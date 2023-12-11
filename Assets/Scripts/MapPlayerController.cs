using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapPlayerController : MonoBehaviour
{

    public Vector2 move;
    public float speed;
    public Rigidbody2D rb;
    public Animator anim;
    public float moveDampTime = 0.1f;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        GameManager.instance.controls.Map.Movement.performed += ctx => move = ctx.ReadValue<Vector2>();
        GameManager.instance.controls.Map.Movement.canceled += ctx => move = Vector2.zero;

        GameManager.instance.controls.Map.Menu.performed += ctx => GameManager.instance.ToggleMenu();
    }

    // Update is called once per frame
    void Update()
    {
        if(move.magnitude > 0)
        {
            rb.velocity = move * speed;
            anim.SetBool("isWalking", true);
            if(move.x!= 0)
            anim.SetFloat("moveX", move.x, moveDampTime, Time.deltaTime );
        }
        else
        {
            rb.velocity = Vector2.zero;
            anim.SetBool("isWalking", false);
        }
    }
}
