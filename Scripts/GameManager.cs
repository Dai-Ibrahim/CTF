using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

public class GameManager : MonoBehaviourPunCallbacks
{
    [Header("Stats")]
	public bool gameEnded = false; // has the game ended?
	public float timeToWin; // time a player needs to hold the hat to win
	public float invincibleDuration; // how long after a player gets the hat, are they invincible
	private float hatPickupTime; // the time the hat was picked up by the current holder
	[Header("Players")]
	public string playerPrefabLocation; // path in Resources folder to the Player prefab
	public Transform[] spawnPoints; // array of all available spawn points
	public PlayerController[] players; // array of all the players
	public int playerWithHat; // id of the player with the hat
	private int playersInGame; // number of players in the game
	
	public Material mat;
	public Color[] color={new Color(0.6509434f,0.2302866f,0.2811725f,1),new Color(0.9339623f,0.3422349f,0.09251513f,1), new Color(0,0.68f,1,1), new Color(0.4644604f,0,1,1)};
	// instance
	public static GameManager instance;
	void Awake ()
	{
		// instance
		instance = this;
	}
    void Start ()
	{
		players = new PlayerController[PhotonNetwork.PlayerList.Length];
		photonView.RPC("ImInGame", RpcTarget.All);
		  
	}

    [PunRPC]
	void ImInGame ()
	{
		playersInGame++;
		if(playersInGame == PhotonNetwork.PlayerList.Length)
		SpawnPlayer();
	}
	// spawns a player and initializes it
	void SpawnPlayer ()
	{
	// instantiate the player across the network
		GameObject playerObj = PhotonNetwork.Instantiate(playerPrefabLocation, spawnPoints[PhotonNetwork.LocalPlayer.ActorNumber -1].position, Quaternion.identity);
		// get the player script
		var playerMat = playerObj.GetComponent<Renderer>();
//		playerMat.material.SetColor("_Color", Color.red);
		playerMat.material.SetColor("_Color", color[PhotonNetwork.LocalPlayer.ActorNumber-1]);
//		Debug.Log("mat");
//		Debug.Log(mat);
		PlayerController playerScript = playerObj.GetComponent<PlayerController>();
		playerScript.photonView.RPC("Initialize", RpcTarget.All, PhotonNetwork.LocalPlayer);
	}
	
	public PlayerController GetPlayer (int playerId)
	{
		return players.First(x => x.id == playerId);
	}
	public PlayerController GetPlayer (GameObject playerObject)
	{
		return players.First(x => x.gameObject == playerObject);
	}
	
	// sets the player's hat active or not
	
	// called when a player hits the hatted player - giving them the hat
	[PunRPC]
	public void GiveHat (int playerId, bool initialGive)
	{
		// remove the hat from the currently hatted player
		if(!initialGive)
			GetPlayer(playerWithHat).SetHat(false);
		// give the hat to the new player
		playerWithHat = playerId;
		GetPlayer(playerId).SetHat(true);
		hatPickupTime = Time.time;
	}
	[PunRPC]
	public void StartCounting (int playerId, bool status)
	{
		// remove the hat from the currently hatted player
		if(!status)
			GetPlayer(playerWithHat).setBase(false);
		// give the hat to the new player
		playerWithHat = playerId;
		Debug.Log(playerId);
		GetPlayer(playerId).setBase(status);
		hatPickupTime = Time.time;
	}
	// is the player able to take the hat at this current time?
	public bool CanGetHat ()
	{
		if(Time.time > hatPickupTime + invincibleDuration)
			return true;
		else
			return false;
	}
	
	[PunRPC]
	void WinGame (int playerId)
	{
		gameEnded = true;
		PlayerController player = GetPlayer(playerId);
		GameUI.instance.SetWinText(player.photonPlayer.NickName);
		Invoke("GoBackToMenu", 3.0f);
	}
	
	void GoBackToMenu ()
	{
		PhotonNetwork.LeaveRoom();
		NetworkManager.instance.ChangeScene("Menu");
	}
	
}
