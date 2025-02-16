using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public partial class GameRoom : JobSerializer
    {


        public void HandleMove(Player player, C_Move movePacket)
        {
            if (player == null)
                return;

            //클라가 보낸 정보가 찐인지 검증

            //일단 서버에서 좌표 이동
            PositionInfo movePosInfo = movePacket.PosInfo; //희망하는 이동좌표
            ObjectInfo info = player.Info; //실제 플레이어 정보

            //다른 좌표로 이동할 경우, 갈 수 있는지 체크
            if (movePosInfo.PosX != info.PosInfo.PosX || movePosInfo.PosY != info.PosInfo.PosY) //진짜로 좌표 이동하는게 맞을 시
            {
                if (Map.CanGo(new Vector2Int(movePosInfo.PosX, movePosInfo.PosY)) == false) //갈수있는 지역있지 체크
                    return; //못지나가는 지역이면 걍 리턴
            }
            //이동가능한경우 현재 상태를 이동하는 상태&방향으로 바꿔줌
            info.PosInfo.State = movePosInfo.State;
            info.PosInfo.MoveDir = movePosInfo.MoveDir;
            Map.ApplyMove(player, new Vector2Int(movePosInfo.PosX, movePosInfo.PosY)); //이동하고싶다고 map에 요청

            //다른플레이어한테도 알려줌
            S_Move resMovePacket = new S_Move();
            resMovePacket.ObjectId = player.Info.ObjectId;
            resMovePacket.PosInfo = movePacket.PosInfo;

            Broadcast(resMovePacket);
        }

        public void HandleSkill(Player player, C_Skill skillPacket)
        {
            if (player == null)
                return;

            ObjectInfo info = player.Info;
            if (info.PosInfo.State != CreatureState.Idle)
                return;

            //TODO : 스킬 사용 가능 여부 체크

            info.PosInfo.State = CreatureState.Skill;
            S_Skill skill = new S_Skill() { Info = new SkillInfo() };
            skill.ObjectId = info.ObjectId;
            skill.Info.SkillId = skillPacket.Info.SkillId;
            Broadcast(skill);

            Data.Skill skillData = null;
            if(DataManager.SkillDict.TryGetValue(skillPacket.Info.SkillId, out skillData) == false)
                return;

            switch(skillData.skillType)
            {
                case SkillType.SkillAuto:
                    {
                        //TODO : 데미지 판정
                        Vector2Int skillPos = player.GetFrontCellPos(info.PosInfo.MoveDir); // 내 앞의 좌표 얻어옴
                        GameObject target = Map.Find(skillPos); //그 좌표에 위치한 오브젝트(몬스터, 플레이어) 얻어옴
                        if (target != null)
                        {
                            Console.WriteLine("Hit GameObject !");
                        }
                    }
                    break;
                case SkillType.SkillProjectile:
                    {
                        Arrow arrow = ObjectManager.Instance.Add<Arrow>();
                        if (arrow == null)
                            return;

                        arrow.Owner = player;
                        arrow.Data = skillData;
                        arrow.PosInfo.State = CreatureState.Moving;
                        arrow.PosInfo.MoveDir = player.PosInfo.MoveDir;
                        arrow.PosInfo.PosX = player.PosInfo.PosX;
                        arrow.PosInfo.PosY = player.PosInfo.PosY;
                        arrow.Speed = skillData.projectile.speed;
                        Push(EnterGame, arrow);
                    }
                    break;
            }
        }

    }
}
