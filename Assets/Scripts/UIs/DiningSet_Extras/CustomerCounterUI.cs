using System;
using UnityEngine;

public class CustomerCounterUI : DeliveryResultUI
{
    [SerializeField] private CustomerCounter customerCounter;
    protected override void Start()
    {
        customerCounter.OnPlayerInteraction += CustomerCounter_OnPlayerInteraction;
        Hide();
    }

    private void CustomerCounter_OnPlayerInteraction(object sender, CustomerCounter.OnPlayerInteractionEventArgs e)
    {
        switch (e.interactionResult)
        {
            case CustomerCounter.InteractionResult.success:

                base.Delivery_OnRecipeSuccess(sender, e);
                break;

            case CustomerCounter.InteractionResult.failure:
                base.Delivery_OnRecipeFailed(sender, e);
                break;

            case CustomerCounter.InteractionResult.empty:

                Show();
                backGroundImage.color = Color.gray;
                messageText.text = "BRING\nDELIVERY";
                iconImage.sprite = failedSprite;
                animator.SetTrigger(POPUPSTRING);
                break;
        }
    }
}
