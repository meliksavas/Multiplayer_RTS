using UnityEngine;
using Photon.Pun;
using Photon.Realtime; 

public class BaseUnit : MonoBehaviourPun
{
    
    [Header("Unit Base Stats")]
    public float health = 100f;
    public float attackDamage = 10f;
    public float moveSpeed = 5f;
    public float attackRange = 1.5f;
    public float movementSpeed = 5f;

    public GameObject Selected;
    public Player Owner { get; private set; }

    void Awake()
    {

  
        Owner = photonView.Owner;


        if (Owner != null)
        {
            Debug.Log($"Unit '{gameObject.name}' created. Owner Actor Number: {Owner.ActorNumber}");
        }
        else
        {
            
            Debug.LogWarning($"Unit '{gameObject.name}' was created without an owner.", this);
        }
    }

    private Vector2 targetPosition;
    private bool isMoving = false;

    public void MoveToPosition(Vector2 position)
    {
        targetPosition = position;
        isMoving = true;
    }

    void Update()
    {
        if (isMoving)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            if (Vector2.Distance(transform.position, targetPosition) < 0.05f)
                isMoving = false;
        }
    }

}
