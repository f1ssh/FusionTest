using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.EventSystems;

public class BasicSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    public static BasicSpawner instance;
    [SerializeField] private NetworkPrefabRef _playerPrefab;
    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();
    private bool _mouseButton0;
    private bool _mouseButton1;
    private bool _shoot;
    private Vector3 _bulletDirection;
    private PlayerInput playerInput;

    private Resolution resolution;
    private float resolutionScale;

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            // Create a unique position for the player
            Vector3 spawnPosition = new Vector3((player.RawEncoded % runner.Config.Simulation.DefaultPlayers) * 3, 1, 0);
            NetworkObject networkPlayerObject = runner.Spawn(_playerPrefab, spawnPosition, Quaternion.identity, player);
            // Keep track of the player avatars so we can remove it when they disconnect
            _spawnedCharacters.Add(player, networkPlayerObject);
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        // Find and remove the players avatar
        if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
        {
            runner.Despawn(networkObject);
            _spawnedCharacters.Remove(player);
        }
    }

    public bool IsPointerOverUI(int pointerId)
    {
        EventSystem eventSystem = EventSystem.current;
        bool data;
        //print(eventSystem.currentSelectedGameObject.name);
        data =  eventSystem.IsPointerOverGameObject(pointerId);
        return data;

    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData();

        /*
        if (Input.GetKey(KeyCode.W))
            data.direction += Vector3.forward;

        if (Input.GetKey(KeyCode.S))
            data.direction += Vector3.back;

        if (Input.GetKey(KeyCode.A))
            data.direction += Vector3.left;

        if (Input.GetKey(KeyCode.D))
            data.direction += Vector3.right;
        */

        

        Vector2 axisInput = playerInput.actions["Move"].ReadValue<Vector2>();
        data.direction = axisInput;

        if(_shoot)
        {
            data.shoot = true;
            data.bulletDirection = _bulletDirection;
        }
        _shoot = false;

        if (_mouseButton0)
            data.buttons |= NetworkInputData.MOUSEBUTTON1;
        _mouseButton0 = false;

        if (_mouseButton1)
            data.buttons |= NetworkInputData.MOUSEBUTTON2;
        _mouseButton1 = false;

        input.Set(data);
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        playerInput = GetComponent<PlayerInput>();
        EnhancedTouchSupport.Enable();
        TouchSimulation.Enable();
        resolution = Screen.currentResolution;
        resolutionScale = resolution.height / 844;
        Camera.main.eventMask = LayerMask.GetMask("UI");
    }

    // Update is called once per frame
    void Update()
    {
        var touches = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches;
        foreach (var touch in touches)
        {

            
            ///*
            if (IsPointerOverUI(touch.touchId))
                continue;

            if (!touch.began)
                continue;

            Vector2 screenPos = touch.screenPosition;

            _bulletDirection = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -Camera.main.transform.position.z));
            //Debug.Log($"{_bulletDirection.x},{_bulletDirection.y},{_bulletDirection.z}");            
            _shoot = true;
            break;

            //*/
        }
    }

    private NetworkRunner _runner;

    private void OnGUI()
    {
        if (_runner == null)
        {
            if (GUI.Button(new Rect(0, 0, 300, 70), "Host"))
            {
                StartGame(GameMode.Host);
            }
            if (GUI.Button(new Rect(0, 70, 300, 70), "Join"))
            {
                StartGame(GameMode.Client);
            }
        }
    }

    async void StartGame(GameMode mode)
    {
        // Create the Fusion runner and let it know that we will be providing user input
        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;

        // Start or join (depends on gamemode) a session with a specific name
        await _runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = "TestRoom",
            Scene = SceneManager.GetActiveScene().buildIndex,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }

    void INetworkRunnerCallbacks.OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        throw new NotImplementedException();
    }

    void INetworkRunnerCallbacks.OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
    {
        throw new NotImplementedException();
    }
}
