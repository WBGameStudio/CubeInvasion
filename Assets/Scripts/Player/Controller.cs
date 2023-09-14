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

    public void ControlingCharacter() 
    {
        //Fixing that, while player not rotating the character still having a velocity with using joystick speeds.
        if(joystick.Horizontal == 0 ||  joystick.Vertical == 0) 
        { 
            rb.velocity = new Vector3(0,rb.velocity.y,0); 
            return; 
        }
        

        Vector3 newPos = new Vector3((joystick.Horizontal * speed),rb.velocity.y, (joystick.Vertical * speed));
        rb.velocity = newPos;
        rb.transform.rotation = Quaternion.LookRotation(newPos);
    }
}
