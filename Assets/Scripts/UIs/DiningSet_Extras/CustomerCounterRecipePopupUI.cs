using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomerCounterRecipePopupUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI recipeNameText;
    [SerializeField] private Transform iconsContainer;
    [SerializeField] private Transform iconTemplate;
    [SerializeField] private CustomerCounter customerCounter;

    private void Awake()
    {
        iconTemplate.gameObject.SetActive(false);
    }

    private void Start()
    {
        customerCounter.OnRecipeRequested += CustomerCounter_OnRecipeRequested;
        customerCounter.OnPlayerInteraction += CustomerCounter_OnPlayerInteraction;
        Hide();
    }

    private void CustomerCounter_OnPlayerInteraction(object sender, CustomerCounter.OnPlayerInteractionEventArgs e)
    {
        if(e.interactionResult == CustomerCounter.InteractionResult.success)
        {
            CustomerCounter_OnRecipeSuccess(sender, e);
        }
        if(e.interactionResult == CustomerCounter.InteractionResult.failure)
        {
            CustomerCounter_OnRecipeFailure(sender, e);
        }
    }

    private void CustomerCounter_OnRecipeFailure(object sender, System.EventArgs e)
    {
        Hide();
    }

    private void CustomerCounter_OnRecipeSuccess(object sender, System.EventArgs e)
    {
        Hide();
    }

    private void CustomerCounter_OnRecipeRequested(object sender, int index)
    {
        Show();
        SetReipeSO(customerCounter.GetRecipeSOForIndex(index));
    }

    public void SetReipeSO(RecipeSO recipeSO)
    {
        recipeNameText.text = recipeSO.RecipeName;
        foreach (Transform t in iconsContainer)
        {
            if (t == iconTemplate) { continue; }
            Destroy(t.gameObject);
        }

        foreach (var kitchenObject in recipeSO.KitchenObjectSOList)
        {
            Transform iconTransform = Instantiate(iconTemplate, iconsContainer);
            iconTransform.gameObject.SetActive(true);

            iconTransform.GetComponent<Image>().sprite = kitchenObject.sprite;
        }
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }
    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
