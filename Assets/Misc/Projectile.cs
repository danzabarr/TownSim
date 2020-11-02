using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Rigidbody rb;
    public ParticleSystem trail;
    public Rigidbody Rigidbody => rb;
    public LayerMask mask;
    public ITarget target;
    public float knockback;
    public float damage;
    public float follow;
    private Vector3 velocity;
    public int faction;

    private Vector3 initialPosition;
    private Vector3 initialVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        initialPosition = transform.position;
        initialVelocity = rb.velocity;
    }

    public void IgnoreCollision(GameObject go)
    {
        foreach(Collider c0 in GetComponentsInChildren<Collider>())
            foreach (Collider c1 in go.GetComponentsInChildren<Collider>())
                Physics.IgnoreCollision(c0, c1);
    }

    private void FixedUpdate()
    {
        velocity = rb.velocity;

        if (rb)
        {
            transform.forward = Vector3.Lerp(transform.forward, rb.velocity, Time.deltaTime * rb.velocity.sqrMagnitude);
            //if (target != null)
            //    rb.MovePosition(transform.position + (target.Center() - transform.position).normalized * follow * Time.deltaTime);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (mask != (mask | (1 << collision.gameObject.layer)))
            return;

        trail.transform.parent = null;

        ContactPoint point = collision.GetContact(0);
        
        transform.position = point.point;
        transform.forward = -collision.relativeVelocity;

        transform.parent = collision.transform;

        ITarget target = collision.transform.GetComponent<ITarget>();

        if (target != null)
        {
            if (target.Faction() != faction)
                target.Damage(damage, velocity.normalized * knockback);
        }
        Destroy(rb);
        foreach(Collider c in GetComponentsInChildren<Collider>())
            Destroy(c);

        Destroy(this);
    }

    private void OnDrawGizmos()
    {
        bool DrawArc(Vector3 p0, Vector3 p1)
        {
            Gizmos.DrawLine(p0, p1);
            return false;
        }

        Ballistics.Arc(initialPosition, initialVelocity, 0, Time.fixedDeltaTime, 1000, DrawArc);
    }
}


