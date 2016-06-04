using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

[RequireComponent(typeof(AudioSource))]
public class LobbyManager : NetworkManager 
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
	private AudioSource			_audio;
	private NetworkStartPosition[] _spawnPoints;

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
		{   
			Debug.LogError("There can only be one instance of MyNetworManager at any time.");
			Destroy(gameObject);
		}
		remoteIpInput.text = GetLocalIPAddress ();
		remoteOnlineIpInput.text = "46.101.17.239";
		_instance = this;
		_currentPanel = startRect;
		_audio = GetComponent<AudioSource> ();
	}

	public void OnCreateHostButtonClick()
	{
		Debug.Log("OnCreateHostButtonClick");
		networkAddress = remoteIpInput.text;
		if (StartHost()!= null)
		{
			_isServer = true;
			StartGame ();
		}
	}

	public void OnCreateClientButtonClick()
	{
		Debug.Log("OnCreateClientButtonClick");
		networkAddress = remoteIpInput.text;
		StartClient();
		StartGame ();
	}

	public void OnBackToStartButtonClick()
	{
		Debug.Log("OnBackToStartButtonClick");
		ChangeTo (startRect);
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
		StartClient ();
		StartGame ();
	}

	public void StartGame() {
		ServerChangeScene(onlineScene);
		// startButton.gameObject.SetActive(false);
		toggleLobbyButton.gameObject.SetActive(true);
		if (titleText.gameObject != null) {
			titleText.gameObject.SetActive (false);
		}
		ShowLobby(false);
		_audio.volume = .2f;
	}

	public void EndGame() {
		Debug.Log("EndGame");

		if (IsHeadless ()) return;

		titleText.gameObject.SetActive (true);
		ChangeTo (startRect);
		DropConnection ();
		_audio.volume = 1;
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
			new Vector3(-1300, 0, 0);
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

//	public override void OnClientSceneChanged(NetworkConnection conn) {
//	}
		
	// Gets called when a client disconnected
	public override void OnClientDisconnect(NetworkConnection conn)
	{
		Debug.Log("OnClientDisconnect");
		base.OnClientDisconnect(conn);
		ChangeTo(startRect);
		titleText.gameObject.SetActive (true);
		ShowLobby(true);
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

	private static string GetLocalIPAddress()
	{
//        IReadOnlyList<HostName> hosts = System.Net.NetworkInformation.GetHostNames();
//        foreach (HostName aName in hosts)
//        {
//            if (aName.Type == HostNameType.Ipv4)
//            {
//                return aName;
//            }
//        }
        return "127.0.0.1";
	}

	void DropConnection() 
	{
		if (IsHeadless ()) return;

		if(_isServer)
			StopHost();
		else 
			StopClient();
		Destroy (this.gameObject);
		_instance = null;
		SceneManager.LoadScene ("Start");
	}

	void OnApplicationQuit()
	{ 
		DropConnection ();
	}

	// detect headless mode (which has graphicsDeviceType Null)
	bool IsHeadless() {
		return SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null;
	}
}