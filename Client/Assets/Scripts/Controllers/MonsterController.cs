using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;
using static Define;

public class MonsterController : CreatureController
{
    Coroutine _coSkill;

    protected override void Init()
    {
        base.Init();
    }

    protected override void UpdateIdle()
    {
        base.UpdateIdle();
    }

    public override void OnDamaged()
    {
        //Managers.Object.Remove(Id); // ȭ�쿡 ���� ���� ������Ʈ(�ڱ��ڽ�) ����
        //Managers.Resource.Destroy(gameObject); // ȭ�쿡 ���� ���� ������Ʈ(�ڱ��ڽ�) ����
    }

    public override void UseSkill(int skillId)
    {
        if (skillId == 1)
        {
            State = CreatureState.Skill;
        }
    }
}
