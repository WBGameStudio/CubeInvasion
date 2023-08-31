using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class Controller : MonoBehaviour
{
    [SerializeField] DynamicJoystick joystick;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Rigidbody rb;

    [SerializeField] float speed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ControlingCharacter();
    }

    public void ControlingCharacter() 
    {
        if(!NavMesh.SamplePosition(agent.nextPosition, out NavMeshHit hit, 1f, NavMesh.AllAreas)) { return; }
        
        Vector3 newPos = new Vector3((joystick.Horizontal * speed),rb.velocity.y, (joystick.Vertical * speed));
        rb.velocity = newPos;
        rb.transform.rotation = Quaternion.LookRotation(newPos);
    }
}
