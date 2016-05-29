using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


public class PlayerListBehaviour : MonoBehaviour 
{        
    public  RectTransform       playersRect;
	public PlayerInfoBehaviour playerInfo;

    private VerticalLayoutGroup _layout;
	private List<PlayerInfoBehaviour>   _playerList = new List<PlayerInfoBehaviour>();


	public List<PlayerInfoBehaviour> PlayerList { get { return _playerList; } }

    public void OnEnable()
    {
        _layout = playersRect.GetComponent<VerticalLayoutGroup>();
    }

    void Update()
    {
        // workaround used in the example network lobby of Unity. 
        // The layout manager doesn't update correctly from time to time
		if (_layout) {
//			_layout.childAlignment = Time.frameCount % 2 == 0 ? 
//                TextAnchor.UpperCenter : 
//                TextAnchor.UpperLeft;
			UpdatePlayerList ();
		}

    }

	public void AddPlayer(PlayerInfoBehaviour lobbyPlayer)
	{
		RectTransform trfrm = lobbyPlayer.gameObject.GetComponent<RectTransform>();
		trfrm.SetParent(playersRect, false);
		_playerList.Add(lobbyPlayer);
	}

	public void ResetPlayerList()
	{
		_playerList = new List<PlayerInfoBehaviour>();
		foreach (Transform t in playersRect.transform) {
			GameObject.Destroy(t.gameObject);
		}
	}

	public void UpdatePlayerList()
	{
		ResetPlayerList ();
		var players = GameObject.FindGameObjectsWithTag("Player");
		foreach(GameObject p in players) {
			StarshipController starShip = p.GetComponent<StarshipController> ();
			if (starShip) {
				PlayerInfoBehaviour info = Instantiate (playerInfo);
				info.SetContent (starShip);
				AddPlayer (info);
			}
		}
	}
}
