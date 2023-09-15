using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class Controller : MonoBehaviour
{
    [SerializeField] DynamicJoystick joystick;
    [SerializeField] Rigidbody rb;

    [SerializeField] float speed;


    public Transform currentTarget = null;
    Vector3 moveDirection;
    private Quaternion targetRotation;
    private Quaternion lookAt;

    // Update is called once per frame
    void Update()
    {
        ControlingCharacter();
        CharacterRotator();
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
        moveDirection = cameraForward * inputDirection.z + Camera.main.transform.right * inputDirection.x;
        moveDirection.Normalize();
        
        rb.velocity = moveDirection * speed;

        
        


    }

    void CharacterRotator() 
    {

        float lookSpeed = 150f;

        if (EnemyOnFOV() || currentTarget != null) 
        {
            Vector3 direction = currentTarget.transform.position - transform.position;
            targetRotation = Quaternion.LookRotation(direction);
            lookAt = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * lookSpeed);
            transform.rotation = lookAt;
        }
        else if(currentTarget == null)
        {
            targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = targetRotation;
        }
    }

    bool EnemyOnFOV() 
    {
        bool check = true;
        FOVManager fovManager = GetComponent<FOVManager>();
        // Detect enemies within the FOV and detection range
        Collider[] colliders = Physics.OverlapSphere(transform.position, fovManager.fov);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                currentTarget = collider.transform;
                check = true;
            }
           
        }

        if(Physics.OverlapSphere(transform.position, fovManager.fov) == null)
        {
            currentTarget = null;
            check = false;
        }
        return check;


    }
}
