using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeliveryManagerSingleUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI recipeNameText;
    [SerializeField] private Transform iconsContainer;
    [SerializeField] private Transform iconTemplate;

    private void Awake()
    {
        iconTemplate.gameObject.SetActive(false);
    }
    public void SetReipeSO(RecipeSO recipeSO)
    {
        recipeNameText.text = recipeSO.RecipeName;
        foreach(Transform t in iconsContainer)
        {
            if(t == iconTemplate) { continue; }
            Destroy(t.gameObject);
        }

        foreach(var kitchenObject in recipeSO.KitchenObjectSOList)
        {
            Transform iconTransform = Instantiate(iconTemplate, iconsContainer);
            iconTransform.gameObject.SetActive(true);

            iconTransform.GetComponent<Image>().sprite = kitchenObject.sprite;
        }
    }
}
