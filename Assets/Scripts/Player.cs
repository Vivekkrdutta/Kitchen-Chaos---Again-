using System;
using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class Player : NetworkBehaviour,IKitchenObjectParent
{
    public static event EventHandler OnAnyPlayerPickedUpSomething;
    public static event EventHandler OnAnyPlayerSpawned;
    public static void ResetStaticData()
    {
        OnAnyPlayerSpawned = null;
        OnAnyPlayerPickedUpSomething = null;
    }

    public event EventHandler OnPickedUpSomething;
    public event EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedCounterChanged;
    public class OnSelectedCounterChangedEventArgs : EventArgs
    {
        public BaseCounter selectedCounter;
    }

    [SerializeField] private PlayerAnimator playerAnimator;
    [SerializeField] private Transform kitchenObjectHoldPoint;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private LayerMask countersLayerMask;
    [SerializeField] private LayerMask collisionsLayerMask;
    [SerializeField] private List<Vector3> spawnPoints;
    [SerializeField] private PlayerVisual playerVisual;
    [SerializeField] private PlayerName playerName;

    private KitchenObject kitchenObject;
    private BaseCounter selectedCounter;

    private float rotateSpeed = 10f;
    private float playerRadius = .7f;
    private Vector3 lastInteractDir;

    [HideInInspector]
    public NetworkVariable<bool> isMoving = new NetworkVariable<bool>(false,readPerm: NetworkVariableReadPermission.Everyone, writePerm: NetworkVariableWritePermission.Owner);

    public static Player LocalInstance { get; private set; }

    private void Start()
    {
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
        GameInput.Instance.OnInteractAlternateAction += GameInput_OnInteractAlternateAction;

        PlayerData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
        int colorId = playerData.colorId;
        playerVisual.SetColor(GameMultiplayer.Instance.GetColorForIndex(colorId));
        playerName.SetPlayerName(playerData.playerName.ToString());
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsOwner)
        {
            LocalInstance = this;
        }
        
        transform.position = spawnPoints[GameMultiplayer.Instance.GetPlayerDataIndexFromClientId(OwnerClientId)];
        OnAnyPlayerSpawned?.Invoke(this,null);

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
        }
    }

    // This function runs on both the server and the client that got disconnected
    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        if(OwnerClientId == clientId && HasKitchenObject()) // If this is the player that disconnected and has a kitchen object
        {
            KitchenObject.DestroyKitchenObject(kitchenObject);
        }
    }

    private void GameInput_OnInteractAlternateAction(object sender, EventArgs e)
    {
        if (GameManager.Instance.IsGamePlaying())
        {
            if(selectedCounter != null)
            {
                selectedCounter.InteractAlternate();
            }
        }
    }
    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        if (GameManager.Instance.IsGamePlaying())
        {
            // if the there is something of a selected counter, invoke its interaction
            if (selectedCounter != null)
            {
                selectedCounter.Interact(this);
            }
        }
    }

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        Vector2 inputVectorNormalized = GameInput.Instance.GetInputVectorNormalized();
        Vector3 moveDir = new Vector3(inputVectorNormalized.x, 0f, inputVectorNormalized.y).normalized;

        HandleMovements(moveDir);  // For players in clientAuthorititive mode
        HandleInteractions(moveDir);

        #region For Animation System

        // Close coupling the player animation system
        if (inputVectorNormalized.magnitude > 0)
        {
            isMoving.Value = true;
            playerAnimator.SetWalkingAnimation(true);
        }
        else if(isMoving.Value) 
        {
            playerAnimator.SetWalkingAnimation(false);
            isMoving.Value = false;
        }
        #endregion
    }

    #region Movements Mechanics
    private void HandleMovements(Vector3 moveDir)
    {

        // Update the rotation
        transform.forward = Vector3.Slerp(transform.forward, moveDir, rotateSpeed * Time.deltaTime);
        float moveDistance = moveSpeed * Time.deltaTime;

        bool canMove = !Physics.BoxCast(
            transform.position, Vector3.one * playerRadius, moveDir, Quaternion.identity,moveDistance,collisionsLayerMask
        );
        if (!canMove)
        {
            Vector3 moveDirX = new Vector3(moveDir.x, 0, 0).normalized;
            Vector3 moveDirZ = new Vector3(0, 0, moveDir.z).normalized;
            // Check if the player can move to the right or left
            if (!Physics.BoxCast(
                transform.position, Vector3.one * playerRadius, moveDirX,Quaternion.identity, moveDistance,collisionsLayerMask
            )){
                canMove = true;
                moveDir = moveDirX;
            }
            // Check if the player can move to the up and down
            else if (!Physics.BoxCast(
                transform.position,Vector3.one * playerRadius, moveDirZ,Quaternion.identity, moveDistance,collisionsLayerMask
            )){
                canMove = true;
                moveDir = moveDirZ;
            }
        }

        // Check if something blocking the way
        if (canMove)
        {
            // Update the position
            transform.position += moveDir * moveSpeed * Time.deltaTime;
        }
    }
    #endregion

    #region Interaction Mechanism
    private void HandleInteractions(Vector3 moveDir)
    {
        //Debug.Log(moveDir);
        if (moveDir != Vector3.zero)
        {
            lastInteractDir = moveDir;
        }
        float interactionDist = 1.5f;
        // Ray cast to check if the player is looking at a counter
        if (Physics.Raycast(transform.position,lastInteractDir, out RaycastHit hitInfo, interactionDist, countersLayerMask))
        {
            // Just for security check to ensure that the object Has a basecounter component
            if (hitInfo.transform.TryGetComponent(out BaseCounter baseCounter))
            {
                // Its a different counter
                if (baseCounter != selectedCounter)
                {
                    // Set the selected counter to a new one
                    SetSelectedCounter(baseCounter);
                }
            }
        }
        // If no counter is hit
        else if (HasAnySelectedCounter())
        {
           
            // else remove reference
            SetSelectedCounter(null);
        }
    }
    #endregion

    private void SetSelectedCounter(BaseCounter selectedCounter)
    {
        this.selectedCounter = selectedCounter;

        OnSelectedCounterChanged?.Invoke(this, new OnSelectedCounterChangedEventArgs()
        {
            selectedCounter = selectedCounter
        });
    }

    private bool HasAnySelectedCounter()
    {
        return selectedCounter != null;
    }

    #region IKitchenObjectParent implementations

    public Transform GetKitchenObjectFollowTransform()
    {
        return kitchenObjectHoldPoint;
    }

    public void SetKitchenObject(KitchenObject kitchenObject)
    {
        this.kitchenObject = kitchenObject;

        if(kitchenObject != null)
        {
            OnPickedUpSomething?.Invoke(this, null);
            OnAnyPlayerPickedUpSomething?.Invoke(this, null);
        }
    }

    public KitchenObject GetKitchenObject()
    {
        return kitchenObject;
    }

    public void ClearKitchenObject()
    {
        kitchenObject = null;
    }

    public bool HasKitchenObject()
    {
        return kitchenObject != null;
    }

    public NetworkObject GetNetworkObject()
    {
        return NetworkObject;
    }

    #endregion
}
