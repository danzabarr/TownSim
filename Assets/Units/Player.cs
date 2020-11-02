using TownSim.Items;
using TownSim.Navigation;
using TownSim.UI;
using TownSim.Units;
using TownSim.Building;
using UnityEngine;
using UnityEngine.EventSystems;

public class Player : MonoBehaviour
{
    private static Player instance;
    public static Player Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<Player>();
            return instance;
        }
    }

    public TownSim.UI.Inventory inventoryUI;
    public TownSim.UI.Inventory offhandUI;

    public Human Unit { get; private set; }
    public Agent Agent { get; private set; }
    private CameraController cam;
    private Rigidbody rb;
    private Animator animator;
    private Vector3 direction;
    [SerializeField] private GameObject aimAssistObject;
    public float movementInputSensitivity;
    private float movementInputSpeed;
    private ITarget aimAssist;
    private bool attackQueued;
    private ITarget queuedTarget;
    [Range(0, 1)] public float queuingWindow;
    public Bar healthBar;
    private float angle;
    public float blendSpeed;
    public float directionalBias;

    private void Awake()
    {
        instance = this;
        Unit = GetComponent<Human>();
        Agent = GetComponent<Agent>();
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        inventoryUI.Contents = Unit.Inventory;
        offhandUI.Contents = Unit.OffHand;

        Unit.Inventory.Add(ItemManager.Type("Sword"), 10);
        Unit.Inventory.Add(ItemManager.Type("Pickaxe"), 10);
        Unit.Inventory.Add(ItemManager.Type("Axe"), 10);
        Unit.Inventory.Add(ItemManager.Type("Shield"), 10);
        Unit.Inventory.Add(ItemManager.Type("Bow"), 10);
        Unit.Inventory.Add(ItemManager.Type("Test Item"), 20);
    }

    private void FixedUpdate()
    {
        if (cam == null)
            cam = Camera.main.GetComponent<CameraController>();

        if (Unit.CurrentHealth() <= 0)
            return;

        //MOVEMENT
        if (!Agent.HasPath) {
            float runSpeed = 0;

            Vector2 forward = cam.transform.forward.xz().normalized;
            Vector2 right = cam.transform.right.xz().normalized;
            Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

            if (Unit.CurrentAction == Action.Idle && input.sqrMagnitude > .00001f)
            {
                movementInputSpeed = Mathf.Lerp(movementInputSpeed, 1, movementInputSensitivity * Time.deltaTime);
                input = input.normalized;

                
                float a = Mathf.Atan2(input.y, input.x);
                a = Mathf.Lerp(angle, a, blendSpeed * Time.deltaTime);
                angle = a;
                float runX = 0;
                float runY = 1;
                directionalBias = 1;

                if (Unit.strafing)
                {
                    runX = Mathf.Cos(a);
                    runY = Mathf.Sin(a);
                    directionalBias = Vector2.Dot(new Vector2(runX, runY), Vector2.up) * .25f + .75f;
                    if (input.y < -0.01f)
                        runX *= -1;
                }

                animator.SetFloat("runX", runX);
                animator.SetFloat("runY", runY);

                runSpeed = Agent.movementSpeed * movementInputSpeed * directionalBias;

                right *= input.x;
                forward *= input.y;

                Vector3 move = (right + forward).x0y();

                float rotation = 90 - Mathf.Atan2(move.z, move.x) * Mathf.Rad2Deg;
                if (Unit.strafing)
                    rotation = cam.yaw;
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.AngleAxis(rotation, Vector3.up), Time.deltaTime * Unit.rotationSpeed);

                //move *= runSpeed * Time.deltaTime;

                float h0 = Map.Instance.MeshHeight(transform.position.x, transform.position.z);
                float h1 = Map.Instance.MeshHeight(transform.position.x + move.x, transform.position.z + move.z);
                move.y = h1 - h0;

                move = move.normalized;
                //if (Physics.Raycast(transform.position, move, out RaycastHit hit, 5, LayerMask.GetMask("Terrain"), QueryTriggerInteraction.Ignore))
                //{
                //    Vector3 temp = Vector3.Cross(hit.normal, move);
                //    move = Vector3.Cross(temp, hit.normal);
                //    Debug.Log(move);
                //}


                direction = move;

                move *= runSpeed * Time.deltaTime;
                //rb.AddForce(move, ForceMode.Acceleration);

                //move = Map.Instance.OnMesh((transform.position + move).xz());
                rb.MovePosition(transform.position + move);
                //rb.AddForce(move, ForceMode.VelocityChange);

                //Unit.MoveToPoint(transform.position + move);
                //MoveToPoint(transform.position + move);
                    
                animator.SetBool("running", true);
                animator.SetFloat("runSpeed", runSpeed);
            }
            else
            {
                animator.SetBool("running", false);
                movementInputSpeed = 0;
                animator.SetFloat("runX", 0);
                animator.SetFloat("runY", 1);
            }
        }
    }

    private void Update()
    {
        healthBar.Fill = Unit.CurrentHealth() / Unit.MaxHealth();


        //if (Input.GetButtonDown("Switch Item"))
        //    unit.CycleItemMode();

        bool overUI = EventSystem.current.IsPointerOverGameObject() || ItemManager.IsHolding || BuildingSystem.IsHolding;

        if (!overUI && Input.GetButtonDown("Pickup"))
            Unit.Pickup();

        if (!overUI && Input.GetButtonDown("Strafe"))
            Unit.strafing = !Unit.strafing;


        //transform.position = Map.Instance.OnMesh(transform.position.xz());

        //if (Input.GetButtonDown("Attack"))
        //{
        //    switch (unit.mode)
        //    {
        //        case Unit.ItemMode.Empty:
        //            break;
        //        case Unit.ItemMode.Sword:
        //            {
        //                if (ScreenCast.MouseScene.Cast(out ITarget target, true) && !target.Equals(this) && target.CurrentHealth() > 0)
        //                    unit.MeleeAttack(target);
        //                else
        //                    unit.MeleeAttack();
        //            }
        //            break;
        //        case Unit.ItemMode.Bow:
        //            {
        //
        //                if (ScreenCast.MouseScene.Cast(out ITarget target, true) && !target.Equals(this) && target.CurrentHealth() > 0)
        //                    aimAssist = target;
        //                else
        //                    aimAssist = unit.RangedTarget();
        //
        //                if (aimAssist != null)
        //                    unit.RangedAttack(aimAssist);
        //                else
        //                    unit.RangedAttack();
        //            }
        //            break;
        //    }
        //}

        aimAssist = null;
        switch (Unit.Mode)
        {
            case ItemMode.Empty:
                break;

            case ItemMode.Axe:
                {
                    if (!overUI && Input.GetMouseButtonDown(0))
                        Unit.Chop();
                }
                break;

            case ItemMode.Pickaxe:
                {
                    if (!overUI && Input.GetMouseButtonDown(0))
                        Unit.Mine();
                }
                break;

            case ItemMode.Sword:
                {
                    Unit.blocking = Input.GetButton("Block");

                    if (attackQueued && Unit.CurrentAction == Action.Idle)
                    {
                        if (queuedTarget != null && queuedTarget.CurrentHealth() > 0)
                            Unit.MeleeAttack(queuedTarget, 1);
                        else
                            Unit.MeleeAttack(1);

                        attackQueued = false;
                        queuedTarget = null;
                    }

                    else if (!overUI && Input.GetMouseButtonDown(0))
                    {
                        if (ScreenCast.MouseScene.Cast(out Unit target, true) && !target.Equals(Unit) && target.CurrentHealth() > 0 && target.Faction() != Unit.Faction())
                        {
                            if (Unit.CurrentAction != Action.Idle)
                            {
                                queuedTarget = target;
                                attackQueued = true;
                            }
                            else
                            {
                                Unit.MeleeAttack(target as ITarget, 1);
                            }
                        }
                        else
                        {
                            if (Unit.CurrentAction != Action.Idle)
                            {
                                queuedTarget = null;
                                attackQueued = true;
                            }
                            else
                            {
                                Unit.MeleeAttack(1);
                            }
                        }
                    }
                    else if (!overUI && Input.GetButtonDown("Attack"))
                    {
                        Unit.MeleeAttack(1);
                    }
                }
                break;
            case ItemMode.Bow:
                {
                    if (ScreenCast.MouseUnit.Cast(out Unit target, true) && !target.Equals(Unit) && target.CurrentHealth() > 0 && target.Faction() != Unit.Faction() && Unit.InRange(target))
                    {
                        aimAssist = target;
                        if (!overUI && Input.GetMouseButtonDown(0))
                            Unit.RangedAttack(target);
                    }

                    else if (Unit.RangedTarget(out target))
                    {
                        aimAssist = target;
                        if (!overUI && Input.GetButtonDown("Attack"))
                            Unit.RangedAttack(target);
                    }

                    else if (!overUI && Input.GetMouseButtonDown(0))
                    {
                        aimAssist = null;

                        bool Shootable(RaycastHit rh)
                        {
                            Unit unit = rh.collider.GetComponentInParent<Unit>();
                            return unit == null || unit != this.Unit;
                        }
                            

                        if (ScreenCast.MouseScene.Cast(out RaycastHit hit) && Unit.InRange(hit.point) && Shootable(hit))
                            Unit.RangedAttack(hit.point);
                        else
                            Unit.RangedAttack();
                    }
                  
                    else if (!overUI && Input.GetButtonDown("Attack"))
                    {
                        aimAssist = null;
                        Unit.RangedAttack();
                    }

                break;
            }
        }
    }

    protected void LateUpdate()
    {
        if (aimAssist == null)
        {
            aimAssistObject.SetActive(false);
        }
        else
        {
            aimAssistObject.SetActive(true);
            aimAssistObject.transform.position = aimAssist.Nameplate();
        }
    }

    protected void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, transform.position + direction);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward);

        if (aimAssist != null)
            Gizmos.DrawSphere(aimAssist.Center(), 1f);


    }
}
