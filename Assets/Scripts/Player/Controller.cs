using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class Controller : MonoBehaviour
{
    [SerializeField] DynamicJoystick joystick;
    [SerializeField] Rigidbody rb;

    [SerializeField] float speed;

    // Update is called once per frame
    void Update()
    {
        ControlingCharacter();
    }

    private void ControlingCharacter() 
    {
        //Fixing that, while player not rotating the character still having a velocity with using joystick speeds.
        if(joystick.Horizontal == 0 ||  joystick.Vertical == 0) 
        { 
            rb.velocity = new Vector3(0,rb.velocity.y,0); 
            return; 
        }
        //Getting the camera's forward
        Vector3 cameraForward = Camera.main.transform.forward;
        cameraForward.y = 0; 
        cameraForward.Normalize();

        //Getting inputDirection
        Vector3 inputDirection = new Vector3(joystick.Horizontal, 0, joystick.Vertical);
        
        //Changing inputDirection according to camera's forward
        Vector3 moveDirection = cameraForward * inputDirection.z + Camera.main.transform.right * inputDirection.x;
        moveDirection.Normalize();
        
        rb.velocity = moveDirection * speed;
        rb.transform.rotation = Quaternion.LookRotation(moveDirection);
        
        // Old movement code:
        
        // Vector3 newPos = new Vector3((joystick.Horizontal * speed),rb.velocity.y, (joystick.Vertical * speed));
        // rb.velocity = newPos;
        
        
    }
}
