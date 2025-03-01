using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;
using static Define;

public class MyPlayerController : PlayerController
{
    bool _moveKeyPressed = false;

    public int WeaponDamage { get; private set; }
    public int ArmorDefence { get; private set; }

    protected override void Init()
    {
        base.Init();
        RefreshAdditionalStat();
    }

    protected override void UpdateController()
    {
        GetUIKeyInput();

        switch (State)
        {
            case CreatureState.Idle:
                GetDirInput();
                break;
            case CreatureState.Moving:
                GetDirInput();
                break;

        }
        base.UpdateController();
    }

    protected override void UpdateIdle()
    {
        //이동 상태로 갈 지 확인
        if (_moveKeyPressed)
        {
            State = CreatureState.Moving;
            return;
        }

        //스킬 상태로 갈 지 확인
        if (Input.GetKey(KeyCode.Space))
        {
            Debug.Log("Skill !");

            C_Skill skill = new C_Skill() { Info = new SkillInfo() };
            skill.Info.SkillId = 2;
            Managers.Network.Send(skill); //서버에 스킬사용요청

            _coSkillCooltime = StartCoroutine("CoInputCooltime", 0.2f);
        }
    }
    Coroutine _coSkillCooltime;
    IEnumerator CoInputCooltime(float time)
    {
        yield return new WaitForSeconds(time);
        _coSkillCooltime = null;
    }

    void LateUpdate()
    {   //카메라 플레이어 따라 이동
        Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
    }

    void GetUIKeyInput()
    {
        if(Input.GetKeyDown(KeyCode.I))
        {
            UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
            UI_Inventory invenUI = gameSceneUI.InvenUI;

            if(invenUI.gameObject.activeSelf) //이미 아이템창이 켜져있다면 끄기
            {
                invenUI.gameObject.SetActive(false);
            }
            else //꺼져있다면 켜기
            {
                invenUI.gameObject.SetActive(true);
                invenUI.RefreshUI();
            }
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
            UI_Stat statUI = gameSceneUI.StatUI;

            if (statUI.gameObject.activeSelf) //이미 아이템창이 켜져있다면 끄기
            {
                statUI.gameObject.SetActive(false);
            }
            else //꺼져있다면 켜기
            {
                statUI.gameObject.SetActive(true);
                statUI.RefreshUI();
            }
        }
    }

    //키보드 입력 받아 이동 방향 설정
    void GetDirInput()
    {
        _moveKeyPressed = true;

        if (Input.GetKey(KeyCode.W))
        {
            Dir = MoveDir.Up;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            Dir = MoveDir.Down;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            Dir = MoveDir.Left;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            Dir = MoveDir.Right;
        }
        else
        {
            _moveKeyPressed = false;
        }
    }

    protected override void MoveToNextPos()
    {
        if (_moveKeyPressed == false) //진짜로 키보드에서 손을 뗀 상태라면
        {
            State = CreatureState.Idle; //멈춤
            CheckUpdatedFlag(); //움직였는지 체크하는 함수 호출
            return;
        }

        Vector3Int destPos = CellPos;

        switch (Dir)
        {
            case MoveDir.Up:
                destPos += Vector3Int.up;
                break;
            case MoveDir.Down:
                destPos += Vector3Int.down;
                break;
            case MoveDir.Left:
                destPos += Vector3Int.left;
                break;
            case MoveDir.Right:
                destPos += Vector3Int.right;
                break;
        }
        if (Managers.Map.CanGo(destPos))
        {
            if (Managers.Object.FindCreature(destPos) == null) // 목적지까지의 길에 오브젝트가 하나도 없다면 그 때 이동
            {
                CellPos = destPos; //실제 이동
            }
        }

        CheckUpdatedFlag(); //움직였는지 체크하는 함수 호출
    }

    protected override void CheckUpdatedFlag()
    {
        if(_updated) //cc의 _updated bool변수가 true가 됐다면 움직인것
        {//움직였다고 서버에 알리기
            C_Move movePacket = new C_Move();
            movePacket.PosInfo = PosInfo;
            Managers.Network.Send(movePacket);
            _updated = false;
        }
    }

    public void RefreshAdditionalStat() //아이템착용에 의한 추가 스탯 갱신
    {
        WeaponDamage = 0;
        ArmorDefence = 0;

        foreach (Item item in Managers.Inven.Items.Values)
        {
            if (item.Equipped == false)
                continue;

            switch (item.ItemType)
            {
                case ItemType.Weapon:
                    WeaponDamage += ((Weapon)item).Damage;
                    break;
                case ItemType.Armor:
                    ArmorDefence += ((Armor)item).Defence;
                    break;
            }
        }
    }
}
