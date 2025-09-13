using TMPro;
using UnityEngine;

public class PlayerName : MonoBehaviour
{
    [SerializeField] private TextMeshPro playerNameText;
    public void SetPlayerName(string playerName)
    {
        playerNameText.text = playerName;
    }

}
