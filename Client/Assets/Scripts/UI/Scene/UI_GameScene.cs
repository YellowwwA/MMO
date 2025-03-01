using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_GameScene : UI_Scene
{
    public UI_Stat StatUI { get; set; }
    public UI_Inventory InvenUI { get; set; }

    public override void Init()
    {
        base.Init();

        StatUI = GetComponentInChildren<UI_Stat>();
        InvenUI = GetComponentInChildren<UI_Inventory>();

        StatUI.gameObject.SetActive(false);
        InvenUI.gameObject.SetActive(false);
    }
}
