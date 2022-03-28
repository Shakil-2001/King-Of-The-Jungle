using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginUI_Manager : MonoBehaviour
{
    public static LoginUI_Manager instance;

    //Screen object variables
    public GameObject loginUI;
    public GameObject registerUI;
    public Text msgText;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != null)
        {
            Debug.Log("Instance already exists!");
            Destroy(this);
        }
    }

    public void LoginScreen()
    {
        loginUI.SetActive(true);
        registerUI.SetActive(false);
        msgText.text = "";
    }

    public void RegisterScreen()
    {
        loginUI.SetActive(false);
        registerUI.SetActive(true);
        msgText.text = "";
    }

    public void LoadMainMenu()
    {
        
    }
}

