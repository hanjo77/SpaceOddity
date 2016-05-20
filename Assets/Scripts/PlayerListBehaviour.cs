using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


public class PlayerListBehaviour : MonoBehaviour 
{        
    public  RectTransform       playersRect;

    private VerticalLayoutGroup _layout;
    private List<LobbyPlayer>   _lobbyPlayerList = new List<LobbyPlayer>();


    public List<LobbyPlayer> LobbyPlayerList { get { return _lobbyPlayerList; } }

    public void OnEnable()
    {
        _layout = playersRect.GetComponent<VerticalLayoutGroup>();
    }

    void Update()
    {
        // workaround used in the example network lobby of Unity. 
        // The layout manager doesn't update correctly from time to time
        if(_layout)
            _layout.childAlignment = Time.frameCount % 2 == 0 ? 
                TextAnchor.UpperCenter : 
                TextAnchor.UpperLeft;
    }

    public void AddPlayer(LobbyPlayer lobbyPlayer)
    {
        RectTransform trfrm = lobbyPlayer.gameObject.GetComponent<RectTransform>();
        trfrm.SetParent(playersRect, false);
        _lobbyPlayerList.Add(lobbyPlayer);
    }
}
