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
        //Managers.Object.Remove(Id); // 화살에 맞은 몬스터 오브젝트(자기자신) 삭제
        //Managers.Resource.Destroy(gameObject); // 화살에 맞은 몬스터 오브젝트(자기자신) 삭제
    }

    public override void UseSkill(int skillId)
    {
        if (skillId == 1)
        {
            State = CreatureState.Skill;
        }
    }
}
