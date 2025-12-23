using UnityEngine;
using Photon.Pun;

public class UnitController : MonoBehaviourPun, IPunObservable
{
    public enum UnitState { Idle, Moving, Chasing, Attacking, Harvesting }
    public UnitState currentState = UnitState.Idle;

    [Header("Stats")]
    public float health = 100f;
    public float moveSpeed = 5f;
    public float attackRange = 2f;
    public float aggroRange = 8f;
    public float attackSpeed = 1f;
    public float damage = 10f;
    public bool isPlayerControlled = false;

    // YENÝ: Sadece bu iþaretli olanlar (Pawn) aðaç kesebilir
    public bool canHarvest = false;

    [Header("Separation")]
    public float separationRange = 0.6f;
    public float separationForce = 1.5f;

    [Header("References")]
    public GameObject selectionCircle;
    public GameObject healthBarPrefab;

    private Animator anim;
    private SpriteRenderer spriteRenderer;

    private float nextAttackTime = 0f;
    private Transform currentTarget;
    private ResourceSource currentResource;
    private Vector3 moveTargetPosition;

    private HealthBar healthBarScript;
    private float maxHealth;

    private Vector3 lastPosition;

    private void Start()
    {
        maxHealth = health;
        moveTargetPosition = transform.position;
        lastPosition = transform.position;

        anim = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (selectionCircle != null) selectionCircle.SetActive(false);

        if (healthBarPrefab != null)
        {
            GameObject barObj = Instantiate(healthBarPrefab, transform.position + new Vector3(0, 1.5f, 0), Quaternion.identity, transform);
            healthBarScript = barObj.GetComponent<HealthBar>();
            healthBarScript.Initialize(health, maxHealth);
        }
    }

    private void Update()
    {
        if (photonView.IsMine)
        {
            StateMachine();
        }
        UpdateAnimations();
    }

    void UpdateAnimations()
    {
        if (anim == null) return;

        bool isMoving = false;
        Vector3 direction = Vector3.zero;

        if (photonView.IsMine)
        {
            isMoving = (currentState == UnitState.Moving ||
                        currentState == UnitState.Chasing ||
                        (currentState == UnitState.Harvesting && currentResource != null && Vector3.Distance(transform.position, currentResource.transform.position) > attackRange));

            if (isMoving)
            {
                direction = (moveTargetPosition - transform.position).normalized;

                if (currentState == UnitState.Chasing && currentTarget != null)
                    direction = (currentTarget.position - transform.position).normalized;
                else if (currentState == UnitState.Harvesting && currentResource != null)
                    direction = (currentResource.transform.position - transform.position).normalized;
            }
        }
        else
        {
            Vector3 diff = transform.position - lastPosition;
            if (diff.sqrMagnitude > 0.0001f)
            {
                isMoving = true;
                direction = diff.normalized;
            }
            lastPosition = transform.position;
        }

        anim.SetBool("IsMoving", isMoving);

        if (isMoving && spriteRenderer != null)
        {
            if (direction.x > 0) spriteRenderer.flipX = false;
            else if (direction.x < 0) spriteRenderer.flipX = true;
        }
    }

    void StateMachine()
    {
        switch (currentState)
        {
            case UnitState.Idle:
                FindTarget();
                break;

            case UnitState.Moving:
                MoveWithSeparation();
                break;

            case UnitState.Chasing:
                if (currentTarget == null) { currentState = UnitState.Idle; return; }
                if (Vector3.Distance(transform.position, currentTarget.position) <= attackRange)
                    currentState = UnitState.Attacking;
                else
                    transform.position = Vector3.MoveTowards(transform.position, currentTarget.position, moveSpeed * Time.deltaTime);
                break;

            case UnitState.Attacking:
                if (currentTarget == null) { currentState = UnitState.Idle; return; }
                if (Vector3.Distance(transform.position, currentTarget.position) > attackRange)
                    currentState = UnitState.Chasing;
                else
                {
                    if (Time.time >= nextAttackTime)
                    {
                        Attack();
                        nextAttackTime = Time.time + 1f / attackSpeed;
                    }
                }
                break;

            case UnitState.Harvesting:
                if (currentResource == null) { currentState = UnitState.Idle; return; }
                if (Vector3.Distance(transform.position, currentResource.transform.position) > attackRange)
                    transform.position = Vector3.MoveTowards(transform.position, currentResource.transform.position, moveSpeed * Time.deltaTime);
                else
                {
                    if (Time.time >= nextAttackTime)
                    {
                        Harvest();
                        nextAttackTime = Time.time + 1f / attackSpeed;
                    }
                }
                break;
        }
    }

    void MoveWithSeparation()
    {
        float dist = Vector3.Distance(transform.position, moveTargetPosition);
        if (dist < 0.1f)
        {
            transform.position = moveTargetPosition;
            isPlayerControlled = false;
            currentState = UnitState.Idle;
            return;
        }

        Vector3 dir = (moveTargetPosition - transform.position).normalized;
        if (dist > 0.5f)
        {
            Vector3 sep = Vector3.zero;
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, separationRange);
            foreach (var h in hits)
            {
                if (h.gameObject != gameObject && h.GetComponent<UnitController>())
                {
                    Vector3 push = transform.position - h.transform.position;
                    sep += push.normalized / (push.magnitude + 0.1f);
                }
            }
            dir += sep * separationForce;
        }
        transform.position += dir.normalized * moveSpeed * Time.deltaTime;
    }

    void FindTarget()
    {
        if (isPlayerControlled) return;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, aggroRange);
        foreach (var h in hits)
        {
            UnitController enemy = h.GetComponent<UnitController>();
            if (enemy != null && !enemy.photonView.IsMine)
            {
                currentTarget = enemy.transform;
                currentState = UnitState.Chasing;
                return;
            }
            MainBuilding enemyBuilding = h.GetComponent<MainBuilding>();
            if (enemyBuilding != null && !enemyBuilding.photonView.IsMine)
            {
                currentTarget = enemyBuilding.transform;
                currentState = UnitState.Chasing;
                return;
            }
        }
    }

    void Attack()
    {
        photonView.RPC("RPC_PlayAttackAnim", RpcTarget.All, "Attack");
        if (currentTarget)
        {
            var unit = currentTarget.GetComponent<UnitController>();
            if (unit != null) { unit.TakeDamage(damage); return; }

            var building = currentTarget.GetComponent<MainBuilding>();
            if (building != null) { building.TakeDamage(damage); return; }
        }
    }

    void Harvest()
    {
        if (currentResource == null) return;
        photonView.RPC("RPC_PlayAttackAnim", RpcTarget.All, "Work");

        int amount = currentResource.Harvest((int)damage);
        if (amount > 0 && ResourceManager.MyInstance != null)
            ResourceManager.MyInstance.AddResource(currentResource.resourceType.ToString(), amount);
        else if (amount == 0)
        {
            currentResource = null;
            currentState = UnitState.Idle;
        }
    }

    [PunRPC]
    void RPC_PlayAttackAnim(string triggerName)
    {
        if (anim != null) anim.SetTrigger(triggerName);
    }

    public void MoveToPosition(Vector3 pos)
    {
        isPlayerControlled = true;
        currentTarget = null;
        currentResource = null;
        pos.z = transform.position.z;
        moveTargetPosition = pos;
        currentState = UnitState.Moving;
    }

    public void SetTarget(Transform target)
    {
        isPlayerControlled = true;
        currentTarget = target;
        currentResource = null;
        currentState = UnitState.Chasing;
    }

    public void SetResourceTarget(ResourceSource resource)
    {
        // YENÝ: Eðer hasat yeteneðim yoksa (Askersem), sadece oraya yürü
        if (!canHarvest)
        {
            MoveToPosition(resource.transform.position);
            return;
        }

        isPlayerControlled = true;
        currentTarget = null;
        currentResource = resource;
        currentState = UnitState.Harvesting;
    }

    public void SetSelected(bool selected) { if (selectionCircle) selectionCircle.SetActive(selected); }
    public void TakeDamage(float amount) { photonView.RPC("RPC_TakeDamage", RpcTarget.All, amount); }

    [PunRPC]
    void RPC_TakeDamage(float amount)
    {
        health -= amount;
        if (healthBarScript) healthBarScript.UpdateBar(health);
        if (health <= 0 && photonView.IsMine) PhotonNetwork.Destroy(gameObject);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) stream.SendNext(health);
        else
        {
            health = (float)stream.ReceiveNext();
            if (healthBarScript) healthBarScript.UpdateBar(health);
        }
    }
}