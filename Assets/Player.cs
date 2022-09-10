using Fusion;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class Player : NetworkBehaviour
{
    [SerializeField] private Ball _prefabBall;
    [SerializeField] private PhysxBall _prefabPhysxBall;
    [SerializeField] private float movementSpeed = 5;
    private Vector3 velocity = Vector3.zero;
    [SerializeField] private float cameraSpeed = 10f;

    private Text _messages;

    [Networked(OnChanged = nameof(OnBallSpawned))]
    public NetworkBool spawned { get; set; }

    private Material _material;
    Material material
    {
        get
        {
            if (_material == null)
                _material = GetComponentInChildren<MeshRenderer>().material;
            return _material;
        }
    }

    [Networked] private TickTimer delay { get; set; }

    private Rigidbody2D _rb;
    private Vector3 _forward;

    public static void OnBallSpawned(Changed<Player> changed)
    {
        changed.Behaviour.material.color = Color.white;
    }

    public override void Render()
    {
        material.color = Color.Lerp(material.color, Color.blue, Time.deltaTime);
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _forward = transform.forward;

        Debug.Log(GetComponent<NetworkObject>().HasInputAuthority);
        if (GetComponent<NetworkObject>().HasInputAuthority)
            Camera.main.transform.position = new Vector3(gameObject.transform.position.x, Camera.main.transform.position.y, gameObject.transform.position.z);
    }

    [Rpc(RpcSources.InputAuthority,RpcTargets.All)]
    public void RPC_SendMessage(string message, RpcInfo info = default)
    {
        if (_messages == null)
            _messages = FindObjectOfType<Text>();
        if (info.IsInvokeLocal)
            message = $"You said: {message}\n";
        else
            message = $"Some other player said: {message}\n";
        _messages.text += message;

    }

    private void Update()
    {
        if (Object.HasInputAuthority && Keyboard.current.rKey.wasPressedThisFrame)
        {
            RPC_SendMessage("Hey Mate!");
        }
    }

    private void FixedUpdate()
    {
        //cam.gameObject.transform.position = new Vector3(transform.position.x, cam.gameObject.transform.position.y, transform.position.z);
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {

            data.direction.Normalize();
            //_cc.Move(5 * data.direction * Runner.DeltaTime);
            //_rb.MovePosition(5 * data.direction * Runner.DeltaTime);
            _rb.velocity = movementSpeed * data.direction;
            //Debug.Log($"{_rb.velocity.x}, {_rb.velocity.y}");

            //if (data.direction.sqrMagnitude > 0)
            //_forward = data.direction;

            if (GetComponent<NetworkObject>().HasInputAuthority)
                Camera.main.transform.position = Vector3.SmoothDamp(Camera.main.transform.position, new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, Camera.main.transform.position.z), ref velocity, 1/cameraSpeed);

                //Camera.main.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, Camera.main.transform.position.z);

            if (delay.ExpiredOrNotRunning(Runner))
            {
                if (data.shoot) // (data.buttons & NetworkInputData.MOUSEBUTTON1) != 0
                {
                    Vector3 heading = data.bulletDirection - transform.position;
                    heading = new Vector3(heading.x, 0, heading.z);
                    float distance = heading.magnitude;
                    _forward = heading / distance;
                    delay = TickTimer.CreateFromSeconds(Runner, 0.5f);
                    Runner.Spawn(_prefabBall,
                    transform.position + _forward, Quaternion.LookRotation(_forward),
                    Object.InputAuthority, (runner, o) =>
                    {
                        // Initialize the Ball before synchronizing it
                        o.GetComponent<Ball>().Init();
                    });
                    spawned = !spawned;
                }
                else if ((data.buttons & NetworkInputData.MOUSEBUTTON2) != 0)
                {
                    delay = TickTimer.CreateFromSeconds(Runner, 0.5f);
                    Runner.Spawn(_prefabPhysxBall,
                        transform.position + _forward,
                        Quaternion.LookRotation(_forward),
                        Object.InputAuthority,
                        (runner, o) =>
                        {
                            o.GetComponent<PhysxBall>().Init(10 * _forward);
                        });
                    spawned = !spawned;
                }
            }
        }
    }
}