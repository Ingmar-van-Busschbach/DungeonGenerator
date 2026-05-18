using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerMovement : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;
    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
        if(camera.TryGetComponent(out MouseClickController mouseClickController))
        {
            mouseClickController.onClickEvent.AddListener(Move);
        }
    }

    private void Move(Vector3 position)
    {
        navMeshAgent.SetDestination(position);
    }
}
