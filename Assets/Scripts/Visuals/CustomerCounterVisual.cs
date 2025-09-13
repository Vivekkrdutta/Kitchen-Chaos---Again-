using UnityEngine;
using System.Collections.Generic;
public class CustomerCounterVisual : MonoBehaviour
{
    private const string HAPPY_TRIGGER = "Happy";
    private const string ANGRY_TRIGGER = "Angry";
    private const string LOOKAROUND_TRIGGER = "LookAround";

    private float lookAroundTimer = 0f;
    private float lookAroundTimerMax = 5f;

    [SerializeField] private List<Animator> customerAnimators;
    [SerializeField] private CustomerCounter customerCounter;

    private void Start()
    {
        customerCounter.OnPlayerInteraction += CustomerCounter_OnPlayerInteraction;
    }

    private void CustomerCounter_OnPlayerInteraction(object sender, CustomerCounter.OnPlayerInteractionEventArgs e)
    {
        switch (e.interactionResult)
        {
            case CustomerCounter.InteractionResult.success:

                CustomerCounter_OnRecipeSuccess(sender, e);
                break;

            case CustomerCounter.InteractionResult.failure:
                CustomerCounter_OnRecipeFailure(sender, e);
                break;

            case CustomerCounter.InteractionResult.empty:

                CustomerCounter_OnRecipeFailure(sender, e);
                break;
        }
    }

    private void Update()
    {
        lookAroundTimer += Time.deltaTime;
        if(lookAroundTimer > lookAroundTimerMax)
        {
            lookAroundTimer = 0f;
            customerAnimators[Random.Range(0, customerAnimators.Count)].SetTrigger(LOOKAROUND_TRIGGER);
            lookAroundTimerMax = Random.Range(1f,3f);
        }
    }

    private void CustomerCounter_OnRecipeFailure(object sender, System.EventArgs e)
    {
        lookAroundTimer = 0f;
        foreach (Animator customerAnimator in customerAnimators)
        {
            customerAnimator.SetTrigger(ANGRY_TRIGGER);
        }
    }

    private void CustomerCounter_OnRecipeSuccess(object sender, System.EventArgs e)
    {
        lookAroundTimer = 0f;
        foreach (Animator customerAnimator in customerAnimators)
        {
            customerAnimator.SetTrigger(HAPPY_TRIGGER);
        }
    }
}
