using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

public class LobbyManager : NetworkLobbyManager 
{
	// singleton static instance
	private static LobbyManager _instance = null;
	public static LobbyManager instance { get { return _instance; } }

	[Space]                     // Creates a space in the inspector UI
	[Header("UI Reference")]    // Creates a title above the following variables
	public RectTransform        networkPanel;
	public RectTransform        startRect;
	public RectTransform        hostAndJoinRect;
	public RectTransform        lobbyRect;
	public RectTransform        playersRect;
	public PlayerListBehaviour  playersListBehaviour;
	public Text                 lobbyTitleText;
	public InputField           remoteIpInput;
	public InputField           remoteOnlineIpInput;
	public InputField           playerNameInput;
	public Button               startButton;
	public Button               toggleLobbyButton;
	public Text					titleText;

	private RectTransform       _currentPanel;
	private bool                _isServer = false;
	private bool                _showLobbyDuringGame = true;
	private bool				_isOnlineClient = false;

	void Awake() {
		if (IsHeadless()) {
			print("headless mode detected");
			StartServer();
		}
	}

	// Gets called on startup
	void Start()
	{   
		if(_instance != null) 
		{   Debug.LogError("There can only be one instance of MyLobbyManager at any time.");
			Destroy(gameObject);
		}
		remoteIpInput.text = GetLocalIPAddress ();
		remoteOnlineIpInput.text = "46.101.17.239";
		_instance = this;
		_currentPanel = startRect;

	}

	public void OnCreateHostButtonClick()
	{
		Debug.Log("OnCreateHostButtonClick");
		networkAddress = remoteIpInput.text;
		if (StartHost()!= null)
		{
			_isServer = true;
			ChangeTo(lobbyRect);

			// the start button remains inactive for all others
			startButton.gameObject.SetActive(true);
			toggleLobbyButton.gameObject.SetActive(false);
		}
	}

	public void OnCreateClientButtonClick()
	{
		Debug.Log("OnCreateClientButtonClick");
		networkAddress = remoteIpInput.text;
		StartClient();
		ChangeTo(lobbyRect);

		// We change the title to "Connecting ..." in case that the host isn't started yet
		lobbyTitleText.text = "Connecting ...";

		toggleLobbyButton.gameObject.SetActive(false);
	}

	public void OnStartButtonClicked()
	{
		Debug.Log("OnStartButtonClicked");
		StartGame ();
	}

	public void OnStopButtonClicked()
	{
		Debug.Log("OnStopButtonClicked");
		EndGame ();
	}

	public void OnPlayLocalButtonClicked()
	{
		Debug.Log("OnPlayLocalButtonClicked");
		ChangeTo (hostAndJoinRect);
	}

	public void OnPlayOnlineButtonClicked()
	{
		Debug.Log("OnPlayOnlineButtonClicked");
		networkAddress = remoteOnlineIpInput.text;
		_isOnlineClient = true;
		StartGame ();
	}

	public void StartGame() {
		ServerChangeScene(playScene);
		startButton.gameObject.SetActive(false);
		toggleLobbyButton.gameObject.SetActive(true);
		titleText.gameObject.SetActive (false);
		ShowLobby(false);
		if (_isOnlineClient) {
			StartClient ();
		}
	}

	public void EndGame() {
		Debug.Log("EndGame");
		if (!IsHeadless ()) {
			if(_isServer)
				StopHost();
			else 
				StopClient();
		}
		ChangeTo (startRect);
	}

	public void OnToggleLobbyButtuonClicked()
	{
		Debug.Log("OnToggleLobbyButtuonClicked");
		_showLobbyDuringGame = !_showLobbyDuringGame;
		ShowLobby(_showLobbyDuringGame);
	}

	// Changes between the hostOrJoinRect and the lobbyRect
	void ChangeTo(RectTransform panel)
	{
		_currentPanel.gameObject.SetActive(false);
		_currentPanel = panel;
		_currentPanel.gameObject.SetActive(true);
	}

	// Moves the lobby recangle in to the right or out to the left
	private void ShowLobby(bool show)
	{
		_showLobbyDuringGame = show;
		RectTransform rt = networkPanel.gameObject.GetComponent<RectTransform>();
		rt.anchoredPosition3D = _showLobbyDuringGame ? 
			new Vector3(0, 0, 0) : 
			new Vector3(-300, 0, 0);
	}



	public override void OnServerReady (NetworkConnection conn)
	{
		Debug.Log("OnServerReady");
		base.OnServerReady (conn);
	}

	///////////////////////////////
	// Overridden Event Handlers //
	///////////////////////////////

	// Gets called when a client has successfully connected to the server
	public override void OnClientConnect(NetworkConnection conn)
	{
		Debug.Log("OnClientConnect");
		base.OnClientConnect(conn);
		ChangeTo(lobbyRect);
		lobbyTitleText.text = "Lobby";
	}

	// Gets called when the client has changed into the game scene
	public override void OnLobbyClientSceneChanged(NetworkConnection conn)
	{
		Debug.Log("OnLobbyClientSceneChanged");
		base.OnLobbyClientSceneChanged(conn);
		if (networkSceneName == offlineScene) {
			ShowLobby(true);
			// ChangeTo (startRect);
			toggleLobbyButton.gameObject.SetActive (false);
			titleText.gameObject.SetActive (true);
		} else {
			ShowLobby(false);
			toggleLobbyButton.gameObject.SetActive(true);
			if (titleText) {
				titleText.gameObject.SetActive (false);
			}
		}
	}

	public override void OnClientSceneChanged(NetworkConnection conn)
	{
		// always become ready.
		if (!ClientScene.ready) {
			ClientScene.Ready(conn);
		}

		if (!this.autoCreatePlayer)
		{
			return;
		}

		bool addPlayer = false;
		if (ClientScene.localPlayers.Count == 0)
		{
			// no players exist
			addPlayer = true;
		}

		bool foundPlayer = false;
		foreach (var playerController in ClientScene.localPlayers)
		{
			if (playerController.gameObject != null)
			{
				foundPlayer = true;
				break;
			}
		}
		if (!foundPlayer)
		{
			// there are players, but their game objects have all been deleted
			addPlayer = true;
		}
		if (addPlayer)
		{
			ClientScene.AddPlayer(0);
		}
		OnLobbyClientSceneChanged (conn);
	}

	// Gets called when a client disconnected
	public override void OnClientDisconnect(NetworkConnection conn)
	{
		Debug.Log("OnClientDisconnect");
		base.OnClientDisconnect(conn);
		// StopClient();
		ChangeTo(startRect);
		titleText.gameObject.SetActive (true);
		ShowLobby(true);
	}

	public override bool OnLobbyServerSceneLoadedForPlayer(GameObject lobbyPlayer, GameObject gamePlayer) {
		return true;
	}
		
	// Gets called when the host has stopped
	public override void OnStopHost()
	{
		Debug.Log("OnStopHost");
		base.OnStopHost();
		if (startButton) {
			startButton.gameObject.SetActive(false);
		}
	}

	public void Respawn(GameObject gameObject) {
		if (NetworkServer.active) {
			NetworkServer.Spawn (gameObject);
		}
	}

//	private void OnLevelWasLoaded(int level)
//	{
//		// NetworkServer.Spawn (gameObject);
//	}

	private static string GetLocalIPAddress()
	{
		var host = Dns.GetHostEntry(Dns.GetHostName());
		foreach (var ip in host.AddressList)
		{
			if (ip.AddressFamily == AddressFamily.InterNetwork)
			{
				return ip.ToString();
			}
		}
		throw new Exception("Local IP Address Not Found!");
	}

	// detect headless mode (which has graphicsDeviceType Null)
	bool IsHeadless() {
		return SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null;
	}
}