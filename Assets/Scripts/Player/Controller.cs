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
    bool isThereKnockBack;

    [SerializeField] float speed;


    public Transform currentTarget = null;
    Vector3 moveDirection;
    private Quaternion targetRotation;
    private Quaternion lookAt;
    public bool check = false;

    // Update is called once per frame
    void Update()
    {
        ControlingCharacter();
        CharacterRotator();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Enemy")) 
        {
            isThereKnockBack = true;
        }
        if (collision.transform.CompareTag("Cube"))
        {
            isThereKnockBack = false;
        }
    }

    



    private void ControlingCharacter() 
    {
       
        //Fixing that, while player not rotating the character still having a velocity with using joystick speeds.
        if (joystick.Horizontal == 0 ||  joystick.Vertical == 0)
        {
            rb.velocity = isThereKnockBack ? new Vector3(rb.velocity.x, rb.velocity.y, rb.velocity.z) : new Vector3(0, rb.velocity.y, 0);
        }
        else if (isThereKnockBack) 
        {
            new Vector3(rb.velocity.x, rb.velocity.y, rb.velocity.z);
        }
        else 
        {
            //Getting the camera's forward
            Vector3 cameraForward = Camera.main.transform.forward;
            cameraForward.y = 0;
            cameraForward.Normalize();

            //Getting inputDirection
            Vector3 inputDirection = new Vector3(joystick.Horizontal, rb.velocity.y, joystick.Vertical);

            //Changing inputDirection according to camera's forward
            moveDirection = cameraForward * inputDirection.z + Camera.main.transform.right * inputDirection.x;
            moveDirection.Normalize();
            rb.velocity = new Vector3(moveDirection.x * speed, rb.velocity.y, moveDirection.z * speed);
        }
        
        





    }

    void CharacterRotator() 
    {

        float lookSpeed = 360f;

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
        check = false;
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
        return check;


    }
}
