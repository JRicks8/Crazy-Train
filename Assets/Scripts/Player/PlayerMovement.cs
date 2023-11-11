using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    float horizontalMove = 0f;

    public float runSpeed = 40f;

    public CharacterController2D controller;

    bool jump = false;

    bool crouch = true;

    public Camera cam;

   

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;

        if (Input.GetButtonDown("Jump"))
        {
            jump = true;
        }

        if (Input.GetButtonDown("Crouch"))
        {
            crouch = true;
        }
        else if (Input.GetButtonUp("Crouch"))
        {
            crouch = false;
        }

    }

    private void FixedUpdate()
    {
        // Move character
        controller.Move(horizontalMove * Time.fixedDeltaTime, crouch, jump);
        jump = false;

       
        
    }
}
