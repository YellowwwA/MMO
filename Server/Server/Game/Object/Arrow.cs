using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf.Protocol;

namespace Server.Game
{
    public class Arrow : Projectile
    {
        public GameObject Owner { get; set; }

        long _nextMoveTick = 0;

        public override void Update() //update문이 너무 자주 실행되지 않게 함
        {
            if (Data == null || Data.projectile == null || Owner == null || Room == null)
                return;

            if (_nextMoveTick >= Environment.TickCount64)
                return;

            long tick = (long)(1000 / Data.projectile.speed);
            _nextMoveTick = Environment.TickCount64 + tick;

            Vector2Int destPos = GetFrontCellPos();
            if(Room.Map.CanGo(destPos))
            {
                CellPos = destPos;

                S_Move movePacket = new S_Move();
                movePacket.ObjectId = Id;
                movePacket.PosInfo = PosInfo;
                Room.Broadcast(movePacket); //화살이 움직임을 모두에게 알림

                Console.WriteLine("Move Arrow");
            }
            else //화살이 갈 수 없음 즉 발사되지 못하고 무언가에 바로 부딪혀 막혔다는 것 (벽 또는 gameobject에 부딪힘)
            {
                GameObject target = Room.Map.Find(destPos);
                if(target != null) // 벽이 아닌 gameObject와 부딪혔다면 피격판정
                {
                    //피격 판정
                    target.OnDamaged(this, Data.damage + Owner.Stat.Attack);
                }
                //화살 소멸
                Room.Push(Room.LeaveGame,Id);
            }
        }

        public override GameObject GetOwner()
        {
            return Owner;
        }
    }
}
