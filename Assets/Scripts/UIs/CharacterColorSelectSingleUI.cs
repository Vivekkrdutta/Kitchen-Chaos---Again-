using UnityEngine;
using UnityEngine.UI;

public class CharacterColorSelectSingleUI : MonoBehaviour
{
    [SerializeField] private int colorId;
    [SerializeField] private Image colorImage;
    [SerializeField] private GameObject selectedGameObject;

    private void Awake()
    {
        GameMultiplayer.Instance.OnPlayerDataNetworkListChanged += GameMultiplayer_OnPlayerDataNetworkListChanged;
        GetComponent<Button>().onClick.AddListener(() => GameMultiplayer.Instance.UpdatePlayerColor(colorId));
    }

    private void GameMultiplayer_OnPlayerDataNetworkListChanged(object sender, System.EventArgs e)
    {
        UpdateSelected();
    }

    private void Start()
    {
        colorImage.color = GameMultiplayer.Instance.GetColorForIndex(colorId);
        UpdateSelected();
    }
    
    private void UpdateSelected()
    {
        if(GameMultiplayer.Instance.GetPlayerData().colorId == colorId) selectedGameObject.SetActive(true);
        else selectedGameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        GameMultiplayer.Instance.OnPlayerDataNetworkListChanged -= GameMultiplayer_OnPlayerDataNetworkListChanged;
    }
}
