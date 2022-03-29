using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.IO;
using Photon.Realtime;
using UnityEngine.UI;

public class GunMechanics : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField] private GameObject ItemHolder;
    [SerializeField] private Transform Gun;
    [SerializeField] private Transform ShootPoint;
    [SerializeField] Item[] items;
    [SerializeField] Canvas Hotbar; 
    [SerializeField] GameObject[] HotbarItems;
    public AudioSource firingSound;
    public AudioSource changeWeapon;

    private int itemIndex = 0;
    private int previousItemIndex = -1;

    private PlayerController LocalPlayer;

    Vector2 direction;
    int dirMultiplier;
    Vector2 netDir;

    PhotonView PV;

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(Gun.transform.rotation);
        }
        else if (stream.IsReading)
        {
            Gun.transform.rotation = (Quaternion)stream.ReceiveNext();
        }
    }

    void Start()
    {
        PV = GetComponent<PhotonView>();
        LocalPlayer = GetComponent<PlayerController>();
    }


    // Update is called once per frame
    void Update()
    {
        if (!LocalPlayer.canShoot)
        {
            Hotbar.gameObject.SetActive(false);
            return;
        }

        Hotbar.gameObject.SetActive(true);

        //Switch to next/previous gun
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (itemIndex == -1)
                itemIndex = 0;
            else if (itemIndex + 1 > items.Length - 1)
                itemIndex = 0;
            else
                itemIndex += 1;

            EquipItem(itemIndex);
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (itemIndex == -1)
                itemIndex = 0;
            else if (itemIndex - 1 < 0)
                itemIndex = items.Length - 1;
            else
                itemIndex -= 1;

            EquipItem(itemIndex);
        }

        //MousePos - Relative to whole screen, Direction - Relative to Player
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        direction = mousePos - (Vector2)Gun.position;

        if (CheckValidDirection())
            FaceMouse();

        if (Input.GetMouseButtonDown(0) && LocalPlayer.canShoot && itemIndex != 0)
        {
            items[itemIndex].Use(dirMultiplier);
            firingSound.Play(0);
            LocalPlayer.canShoot = false;
            if (itemIndex != 1) //If the weapon isn't a pistol
            {
                LocalPlayer.canMove = false;
            }
        }
    }

    //Face Mouse according to local scale. Stops gun from doing '360'. 
    void FaceMouse()
    {
        if (transform.localScale == Vector3.one)
        {
            dirMultiplier = 1;
            Gun.transform.right = dirMultiplier * direction;
        }
        else
        {
            dirMultiplier = -1;
            Gun.transform.right = dirMultiplier * direction;
        }

    }

    //Checks if gun is pointed in the same direction as player
    bool CheckValidDirection()
    {
        //if player is looking right and mouse is pointed right of player - true else false
        if (direction.x >= 0 && transform.localScale == Vector3.one)
            return true;
        else if (direction.x <= 0 && transform.localScale == new Vector3(-1, 1, 1))
            return true;
        else
            return false;
    }

    //Equip player weapon

    public void EquipItem(int _index)
    {
        //If the player is trying to switch to the weapon already equipped return
        if (_index == previousItemIndex)
            return;

        itemIndex = _index;

        ShowItem(itemIndex, true);
        changeWeapon.Play(0);
        HotbarItems[itemIndex].gameObject.SetActive(true);


        //If the previous item is showing, hide the gameObject
        if (previousItemIndex != -1)
        {
            ShowItem(previousItemIndex, false);
            HotbarItems[previousItemIndex].gameObject.SetActive(false);
        }
            

        previousItemIndex = itemIndex;

        Debug.Log("Current index");
    }

    public void ShowItem(int index, bool show)
    {
        PV.RPC("RPC_ShowItem", RpcTarget.All, index, show);
    }

    [PunRPC]
    void RPC_ShowItem(int index, bool show)
    {
        items[index].itemGameObject.SetActive(show);
    }

}


