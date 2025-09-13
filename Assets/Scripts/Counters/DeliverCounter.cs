using UnityEngine;

public class DeliverCounter : BaseCounter
{
    public static DeliverCounter Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }
    public override void Interact(Player player)
    {
        //if (player.HasKitchenObject())
        //{
        //    if(player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plate))
        //    {
        //        DeliveryManager.Instance.DeliverRecipe(plate);
        //        player.GetKitchenObject().DestroySelf();
        //    }
        //}
    }
}
