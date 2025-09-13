using Unity.Netcode;
using UnityEngine;

public class KitchenObject : NetworkBehaviour
{  
    [SerializeField] private KitchenObjectSO kitchenObjectSO;
    private FollowTransform followTransform;

    protected virtual void Awake()
    {
        followTransform = GetComponent<FollowTransform>();
    }
    private IKitchenObjectParent kitchenObjectParent;
    public void SetKitchenObjectParent(IKitchenObjectParent kitchenObjectParent) // It needs to tell the server
    {
        SetKitchenObjectParentServerRpc(kitchenObjectParent.GetNetworkObject());
    }

    [ServerRpc(RequireOwnership =false)]
    private void SetKitchenObjectParentServerRpc(NetworkObjectReference kitchenObjectParentNetworkReference) // Tell call clients
    {
        SetKitchenObjectParentClientRpc(kitchenObjectParentNetworkReference);
    }

    [ClientRpc]
    private void SetKitchenObjectParentClientRpc(NetworkObjectReference kitchenObjectParentNetworkReference)
    {
        if(kitchenObjectParentNetworkReference.TryGet(out NetworkObject networkObject))
        {
            IKitchenObjectParent kitchenObjectParent = networkObject.GetComponent<IKitchenObjectParent>();
            if (!kitchenObjectParent.HasKitchenObject())
            {
                if (this.kitchenObjectParent != null)
                {
                    this.kitchenObjectParent.ClearKitchenObject();
                }
                this.kitchenObjectParent = kitchenObjectParent;
                this.kitchenObjectParent.SetKitchenObject(this);

                followTransform.SetFollowTransform(kitchenObjectParent.GetKitchenObjectFollowTransform());
            }
        }
    }
    public IKitchenObjectParent GetKitchenObjectParent()
    {
        return kitchenObjectParent;
    }
    public KitchenObjectSO GetKitchenObjectSO()
    {
        return kitchenObjectSO;
    }

    public void DestroySelf()
    {
        DestroyKitchenObject(this);
    }

    public static void SpawnKitchenObject(KitchenObjectSO kitchenObjectSOType, IKitchenObjectParent kitchenObjectParent)
    {
        GameMultiplayer.Instance.SpawnKitchenObject(kitchenObjectSOType, kitchenObjectParent);
    }

    public static void DestroyKitchenObject(KitchenObject kitchenObject)
    {
        GameMultiplayer.Instance.DestroyKitchenObject(kitchenObject);
    }

    public bool TryGetPlate(out PlateKitchenObject plateKitchenObject)
    {
        if(this is PlateKitchenObject)
        {
            plateKitchenObject = this as PlateKitchenObject;
            return true;
        }
        plateKitchenObject = null;
        return false;
    }
}
