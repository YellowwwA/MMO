using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginScene : BaseScene
{
    UI_LoginScene _sceneUI;

    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Login;

        //TODO
        Managers.Web.BaseUrl = "https://localhost:5001/api";


        Screen.SetResolution(1080, 720, false); // 시작 게임화면 크기 설정

        _sceneUI = Managers.UI.ShowSceneUI<UI_LoginScene>();

    }

    public override void Clear()
    {
        
    }
}
