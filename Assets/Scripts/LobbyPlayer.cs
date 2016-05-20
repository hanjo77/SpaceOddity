using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class LobbyPlayer : NetworkLobbyPlayer 
{
    public RectTransform kickButton;
    public Text nameLabel;

    [SyncVar(hook = "OnNameChanged")]
    public string playerName;

    void Start()
    {
        // activate kick buttons for non local players if we're the server
        if(isServer && !isLocalPlayer) 
            kickButton.gameObject.SetActive(true);

        // make sure the name label contains the correct value on creation of the player info object
        OnNameChanged(playerName);
    }

    public override void OnClientEnterLobby()
    {
        Debug.Log("OnClientEnterLobby");
        base.OnClientEnterLobby();
        LobbyManager.instance.playersListBehaviour.AddPlayer(this);
    }

    public override void OnStartLocalPlayer()
    {
        Debug.Log("OnStartLocalPlayer");
        nameLabel.fontStyle = FontStyle.Bold;
        string name = LobbyManager.instance.playerNameInput.text;

        // Check if player name is unique
        bool nameIsUnique = false;
        while (!nameIsUnique)
        {   nameIsUnique = true;
            foreach (LobbyPlayer lp in LobbyManager.instance.playersListBehaviour.LobbyPlayerList)
            {   if (lp.playerName == name)
                {   name += " 2";
                    nameIsUnique = false;
                    break;
                }
            }
        }
            
        CmdSetName(name);
    }

    public void OnNameChanged(string newName)
    {
        Debug.Log("OnNameChanged: " + newName);
        playerName = newName;
        nameLabel.text = playerName;
    }

    [Command]
    public void CmdSetName(string newName)
    {
        Debug.Log("CmdSetName");
        playerName = newName;
    }
}