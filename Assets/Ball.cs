using Fusion;
using UnityEngine;

public class Ball : NetworkBehaviour
{
    [Networked] private TickTimer life { get; set; }
    private Rigidbody2D _rb;
    [SerializeField] private float movementSpeed = 5;

    private Vector3 direction;
    int bounceCount = 0;
    public int maxBounces = 3;

    public void Init()
    {
        life = TickTimer.CreateFromSeconds(Runner, 10.0f);
        _rb = GetComponent<Rigidbody2D>();
        direction = transform.forward;
    }

    public override void FixedUpdateNetwork()
    {
        if (life.Expired(Runner))
            Runner.Despawn(Object);
        else
            _rb.velocity = movementSpeed * direction;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {

        if (collision.transform.tag == "Player")
        {
            Runner.Despawn(Object);
        }
        else if (collision.transform.tag == "Bullet")
        {
            Runner.Despawn(Object);
        }

        else
        {
            if (bounceCount < maxBounces)
            {
                direction = Vector3.Reflect(direction, collision.contacts[0].normal);
                bounceCount++;
            }
            else
            {
                Runner.Despawn(Object);
            }
        }
    }
}