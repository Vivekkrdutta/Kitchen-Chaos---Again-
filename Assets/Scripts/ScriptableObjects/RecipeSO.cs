using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(fileName = "RecipeSO", menuName = "Scriptable Objects/RecipeSO")]
public class RecipeSO : ScriptableObject
{
    [SerializeField] private List<KitchenObjectSO> kitchenObjectSOList;
    [SerializeField] private string recipeName;
    public string RecipeName { get { return recipeName; } private set { } }
    public List<KitchenObjectSO> KitchenObjectSOList { get
        {
            return kitchenObjectSOList;
        }
        private set { }
    }
}
