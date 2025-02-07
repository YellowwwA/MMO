using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf.Protocol;
using Server.Data;

namespace Server.Game
{
    public class Monster : GameObject
    {
        public Monster()
        {
            ObjectType = GameObjectType.Monster;

            //Temp
            Stat.Level = 1;
            Stat.Hp = 100;
            Stat.MaxHp = 100;
            Stat.Speed = 5.0f;

            State = CreatureState.Idle;
        }

        //FSM(Finite State Machine) AI움직임
        public override void Update()
        {
            switch(State)
            {
                case CreatureState.Idle:
                    UpdateIdle();
                    break;                
                case CreatureState.Moving:
                    UpdateMoving();
                    break;                
                case CreatureState.Skill:
                    UpdateSkill();
                    break;                
                case CreatureState.Dead:
                    UpdateDead();
                    break;
            }
        }

        Player _target;
        int _searchCellDist = 10;
        int _chaseCellDist = 20;

        long _nextSearchTick = 0;
        protected virtual void UpdateIdle()
        {
            if (_nextSearchTick > Environment.TickCount64) //1초가 지나지 않았으면 이 if문에 걸려 return됨
                return;
            _nextSearchTick = Environment.TickCount64 + 1000; //다음 기다리는 시간 1초로 재설정

            Player target = Room.FindPlayer(p => //내(몬스터) 주변에 있는 플레이어 탐색
            {
                Vector2Int dir = p.CellPos - CellPos;
                return dir.cellDistFromZero <= _searchCellDist; //일정거리 안에 있으면 찾음
            });

            if (target == null) //못찾았다면 주변에 플레이어 없으므로 걍 리턴
                return;

            //찾았다면
            _target = target;
            State = CreatureState.Moving; //플레이어(타겟)를 쫓아가도록 State를 Moving으로 변경
        }

        int _skillRange = 1;
        long _nextMoveTick = 0;
        protected virtual void UpdateMoving()
        {
            if (_nextMoveTick > Environment.TickCount64) //1초가 지나지 않았으면 이 if문에 걸려 return됨
                return;
            int moveTick = (int)(1000 / Speed);
            _nextMoveTick = Environment.TickCount64 + moveTick; //다음 기다리는 시간 재설정

            if(_target == null || _target.Room != Room) //타겟이 없다면 || 내가 쫓고 있던 플레이어가 나가버리거나 다른 지역으로 이동한다면
            {
                _target = null;
                State = CreatureState.Idle;
                BroadcastMove();
                return;
            }

            Vector2Int dir = _target.CellPos - CellPos;
            int dist = dir.cellDistFromZero;
            if(dist == 0 || dist > _chaseCellDist) //플레이어가 너무 멀리 도망갔다면
            {
                _target = null;
                State = CreatureState.Idle;
                BroadcastMove();
                return;
            }

            List<Vector2Int> path = Room.Map.FindPath(CellPos, _target.CellPos, checkObjects: false);
            if(path.Count<2 || path.Count> _chaseCellDist) //이동할곳이 없거나 너무 멀리있다면
            {
                _target = null;
                State = CreatureState.Idle;
                BroadcastMove();
                return;
            }

            //스킬로 넘어갈지 체크
            if(dist <= _skillRange && (dir.x == 0 || dir.y == 0)) //스킬사용가능 범위 안 일때 + 타겟과 일직선상에 있을때
            {
                _coolTick = 0;
                State = CreatureState.Skill;
                return;
            }

            //이동
            Dir = GetDirFromVec(path[1] - CellPos); // 목적지 - 내현재위치 =>바라볼 방향(플레이어가 있는 방향)
            Room.Map.ApplyMove(this, path[1]);
            BroadcastMove();
        }        
        void BroadcastMove()
        {
            //다른 플레이어한테도 내가 이동함을 알려줌
            S_Move movePacket = new S_Move();
            movePacket.ObjectId = Id;
            movePacket.PosInfo = PosInfo;
            Room.Broadcast(movePacket);
        }

        long _coolTick = 0;
        protected virtual void UpdateSkill()
        {
            if(_coolTick == 0)
            {
                // 유효한 타겟인지 (타겟이 없거나, 같은방이 아니거나, 타겟이 죽었다면 그냥 리턴)
                if(_target == null || _target.Room != Room || _target.Hp == 0)
                {
                    _target = null;
                    State = CreatureState.Moving;
                    BroadcastMove();
                    return;
                }

                // 스킬이 아직 사용 가능한지
                Vector2Int dir = (_target.CellPos - CellPos);
                int dist = dir.cellDistFromZero;
                bool canUseSkill = (dist <= _skillRange && (dir.x == 0 || dir.y == 0));
                if(canUseSkill == false)
                {
                    State = CreatureState.Moving;
                    BroadcastMove();
                    return;
                }

                // 타게팅 방향 주시
                MoveDir lookDir = GetDirFromVec(dir);
                if(Dir != lookDir)
                {
                    Dir = lookDir;
                    BroadcastMove();
                }

                Skill skillData = null;
                DataManager.SkillDict.TryGetValue(1, out skillData); //1번스킬의 데이터(ex쿨타임, 데미지)를 가져옴

                // 데미지 판정
                _target.OnDamaged(this, skillData.damage + Stat.Attack);

                // 스킬 사용 Broadcast
                S_Skill skill = new S_Skill() { Info = new SkillInfo() };
                skill.ObjectId = Id;
                skill.Info.SkillId = skillData.id;
                Room.Broadcast(skill);

                // 스킬 쿨타임 적용
                int coolTick = (int)(1000 * skillData.cooldown);
                _coolTick = Environment.TickCount64 + coolTick;
            }

            if (_coolTick > Environment.TickCount64)
                return;

            _coolTick = 0;
        }        
        protected virtual void UpdateDead()
        {
            
        }

    }
}
