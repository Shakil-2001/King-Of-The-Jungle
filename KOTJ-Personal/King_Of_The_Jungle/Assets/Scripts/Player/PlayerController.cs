using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System;
using TMPro;
using UnityEngine.UI;

public class PlayerController : MonoBehaviourPunCallbacks, IOnEventCallback, IDamageable
{
    private const int TURN_CHANGE = 1;
    private const int WINNER = 2;

    RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All }; //Send event to all clients

    private Rigidbody2D body;
    private Animator anim;
    private PhotonView PV;

    [SerializeField] private float speed;
    [SerializeField] private float jumpPower;
    [SerializeField] int maxJumps;
    [SerializeField] Slider slider; 
    private int jumpCount;
    private bool grounded;
    
    private bool canMove = false;
    public bool canShoot = false; 
    private bool master;

    private const float maxHealth = 100;
    public float health;

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

    private void Awake() //Called Everytime script is loaded
    {
        body = GetComponent<Rigidbody2D>(); //Gets component from game object from inspector tab
        anim = GetComponent<Animator>();
        PV = GetComponent<PhotonView>();
        body.freezeRotation = true;
        jumpCount = maxJumps;
        gameObject.tag = "Player";

        if (PV.IsMine && PhotonNetwork.IsMasterClient)
            master = true;
        else
            master = false;
    }

    private void Update() //Runs every frame
    {
        if (!PV.IsMine)
            return;

        if (!canMove)
        {
            canShoot = false; //Just to ensure it is false when player cannot move
            return;
        }
            

        float horizontalInput = Input.GetAxis("Horizontal"); //Store horizontal Input (-1, 0 ,1)

        body.velocity = new Vector2(horizontalInput * speed, body.velocity.y);

        //Flip player when changing direction
        if (horizontalInput > 0.01f)
            transform.localScale = Vector3.one;
        else if (horizontalInput < -0.01f)
            transform.localScale = new Vector3(-1, 1, 1);

        //Jump - GetKeyDown used to only register the initial click, not holding the space bar
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (jumpCount > 0)
                Jump();
        }

        //If character is moving, turn run animation on, else turn it off
        anim.SetBool("Run", horizontalInput != 0);
        anim.SetBool("Grounded", grounded);

        if (health <= 0)
            Die();
    }

    private void Jump()
    {
        body.velocity = new Vector2(body.velocity.x, jumpPower);
        anim.SetTrigger("Jump");
        jumpCount -= 1;
        grounded = false;

        Debug.Log(PhotonNetwork.PlayerList[0].ToString());
        Debug.Log(PhotonNetwork.NickName); 
    }

    //When player collides with Ground, reset number of jumps
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ground")
            grounded = true;
        jumpCount = maxJumps;
    }

    //Photon - Deals with events that are called
    public void OnEvent(EventData photonEvent)
    {
        //TURN CHANGE EVENT - CALL SENT BY GAMEMANAGER
        if (photonEvent.Code == TURN_CHANGE)
        {
            object[] data = (object[])photonEvent.CustomData;

            //Index 0 - Master, Index 1 - Other Player
            if (master)
            {
                canMove = (bool)data[0];
                canShoot = (bool)data[0];
            }
            else
            {
                canMove = (bool)data[1];
                canShoot= (bool)data[1];
            }
        }
    }

    
    public void TakeDamage(float damage)
    {
        PV.RPC("RPC_TakeDamage", RpcTarget.All, damage);
    }

    [PunRPC]
    void RPC_TakeDamage(float damage)
    {
        health -= damage;
        slider.value = 1 - (health / maxHealth);
    }

    void Die()
    {
        object[] winner = new object[] { PhotonNetwork.NickName, PhotonNetwork.PlayerListOthers[0].ToString()};
        PhotonNetwork.RaiseEvent(WINNER, winner, raiseEventOptions, SendOptions.SendReliable);
    }

}
