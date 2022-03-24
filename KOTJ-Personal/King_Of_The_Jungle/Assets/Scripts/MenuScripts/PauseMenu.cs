using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using Photon.Pun;
using ExitGames.Client.Photon;

public class PauseMenu : MonoBehaviourPunCallbacks, IOnEventCallback
{

    private const int QUIT = 3;

    RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All }; //Send event to all clients

    public static bool GameIsPaused = false;
    public GameObject pauseMenuUI;

    //Needed for handling Events
    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused)
                Resume();
            else
                Pause();
        }

    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        //Time.timeScale = 1f;
        GameIsPaused = false;
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        //Time.timeScale = 0f; - Cant pause time as it's multiplayer
        GameIsPaused = true;

        Debug.Log(PhotonNetwork.AutomaticallySyncScene);
    }

    public void LoadMenu()
    {
        //Time.timeScale = 1f;
        SendQuit();
        Destroy(GameManager.Instance.gameObject);
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("ConnectLobby");
    }

    public void QuitGame()
    {
        SendQuit();
        Destroy(GameManager.Instance.gameObject);
        PhotonNetwork.LeaveRoom();
        Application.Quit();
    }

    public void VictoryLoadMenu()
    {
        //Time.timeScale = 1f;
        Destroy(GameManager.Instance.gameObject);
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("ConnectLobby");
    }

    public void VictoryQuitGame()
    {
        Destroy(GameManager.Instance.gameObject);
        PhotonNetwork.LeaveRoom();
        Application.Quit();
    }

    public void OnEvent(EventData photonEvent)
    {
        return;
    }

    private void SendQuit()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            object[] quit = new object[] { PhotonNetwork.NickName, PhotonNetwork.PlayerListOthers[0].ToString() };
            PhotonNetwork.RaiseEvent(QUIT, quit, raiseEventOptions, SendOptions.SendReliable);
        }
    }
        
}
