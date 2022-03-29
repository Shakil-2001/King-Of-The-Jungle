using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ConnectManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject ConnectPanel;
    [SerializeField] private GameObject StartGameButton;
    [SerializeField] private Text StatusText;
    [SerializeField] private Text WelcomeMessage;

    private const string gameVersion = "1.0";

    private string[] Maps = new string[] {"Game2", "Game3" }; 

    void Awake()
    {
        //Syncs master scene to everyone else
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.GameVersion = gameVersion;
        ShowStatus("Connecting to Photon Servers...");
    }

    private void Update()
    {
        WelcomeMessage.text = "Welcome " + PlayerPrefs.GetString("PlayerName");

        if (PhotonNetwork.NickName == "")
            PhotonNetwork.NickName = PlayerPrefs.GetString("PlayerName");
    }

    //Called when Connect Button is clicked
    public void StartGame()
    {
        ConnectPanel.SetActive(false);
        ShowStatus("Connecting...");

        if (PhotonNetwork.IsConnected)
        {
            ShowStatus("Joining Random Room...");
            PhotonNetwork.JoinRandomRoom();
        }
        else //Connect to Photon Servers
        {
            ShowStatus("Connecting...");
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = gameVersion;
        }
    }


    private void ShowStatus(string text)
    {
        if (StatusText == null)
        {
            return; //do nothing
        }

        //Show the status message and update it with the text passed
        StatusText.gameObject.SetActive(true);
        StatusText.text = text;
    }

    //If connect Button clicked and Photon server joined, Join a random room 
    public override void OnConnectedToMaster()
    {
        ShowStatus("Connected to Servers");
        StartGameButton.SetActive(true);
        WelcomeMessage.text = "Welcome " + PlayerPrefs.GetString("PlayerName");
        PhotonNetwork.NickName = PlayerPrefs.GetString("PlayerName");
        Debug.Log(PhotonNetwork.NickName);
    }

    //If no room available, create new room
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log(message);
        ShowStatus("Creating a new room...");
        PhotonNetwork.CreateRoom(null, new Photon.Realtime.RoomOptions { MaxPlayers = 2 });
    }

    //If Disconnected or no room made/found, take back to Lobby
    public override void OnDisconnected(DisconnectCause cause)
    {
        ConnectPanel.SetActive(true); 
    }

    //When room has been joined
    public override void OnJoinedRoom()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        ShowStatus("Joined room - waiting for another player.");
    }

    //Once 2 players in a room, master client changes everyone to the game scene
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        base.OnPlayerEnteredRoom(newPlayer);
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2 && PhotonNetwork.IsMasterClient)
        {
            string scene = Maps[Random.Range(0, 2)].ToString().Replace("'", "");
            //Used instead of SceneManager.LoadScene, Using PhotonsLoadLevel ensures all players load into the new scene. Look at Awake()
            PhotonNetwork.LoadLevel(scene);
            //LoadScene(scene);

        }
    }
}
