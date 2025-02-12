using System.Collections;
using System.Collections.Generic;
//using ServerCore;
using UnityEngine;

public class StartButton : MonoBehaviour
{
    public GameObject StartPanel;
    //public Connector _connector;

    public void OnJoinedRoom()
    {
        StartPanel.SetActive(false);
        //_connector._startbtn = true;
    }
}
