using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf.Protocol;

namespace Server.Game
{
    public class GameObject
    {
        public GameObjectType ObjectType { get; protected set; } = GameObjectType.None;
        public int Id
        {
            get { return Info.ObjectId; }
            set { Info.ObjectId = value; }
        }
        public GameRoom Room { get; set; }

        public ObjectInfo Info { get; set; } = new ObjectInfo();
        public PositionInfo PosInfo { get; private set; } = new PositionInfo();
        public StatInfo Stat { get; private set; } = new StatInfo();

        public float Speed
        {
            get { return Stat.Speed;}
            set { Stat.Speed = value;}
        }
        public int Hp
        {
            get { return Stat.Hp; }
            set { Stat.Hp = Math.Clamp(value, 0, Stat.MaxHp); } //Clamp함수 : value 값이 0보다 작아지면 0으로 세팅, Max보다 커지면 Max로 세팅
        }
        public MoveDir Dir
        {
            get { return PosInfo.MoveDir; }
            set { PosInfo.MoveDir = value; }
        }

        public CreatureState State
        {
            get { return PosInfo.State; }
            set { PosInfo.State = value; }
        }
        public GameObject()
        {
            Info.PosInfo = PosInfo;
            Info.StatInfo = Stat;
        }
        public virtual void Update()
        {

        }
        public Vector2Int CellPos
        {
            get
            {
                return new Vector2Int(PosInfo.PosX, PosInfo.PosY);
            }
            set
            {
                PosInfo.PosX = value.x;
                PosInfo.PosY = value.y;
            }
        }
        public Vector2Int GetFrontCellPos()
        {
            return GetFrontCellPos(PosInfo.MoveDir);
        }
        public Vector2Int GetFrontCellPos(MoveDir dir) //몬스터 피격 판정을 위해 내 위치 바로 앞의 좌표를 반환 (이후 PC코드에서 이 좌표에 위치하고있는 몬스터 검색)
        {
            Vector2Int cellPos = CellPos; //현재 나의 위치에서 나의 앞

            switch (dir)              //즉, 마지막으로 바라본 방향의 앞에 있는 좌표 얻어옴
            {
                case MoveDir.Up:
                    cellPos += Vector2Int.up;
                    break;
                case MoveDir.Down:
                    cellPos += Vector2Int.down;
                    break;
                case MoveDir.Left:
                    cellPos += Vector2Int.left;
                    break;
                case MoveDir.Right:
                    cellPos += Vector2Int.right;
                    break;
            }

            return cellPos;
        }
        public static MoveDir GetDirFromVec(Vector2Int dir) //몬스터가 어떤 방향 바라봐야 할지 반환해줌
        {
            // dir값(목표지)으로 몬스터가 이동하거나 공격할 방향을 바라보고 이동 및 공격하게 설정
            if (dir.x > 0)
                return MoveDir.Right;
            else if (dir.x < 0)
                return MoveDir.Left;
            else if (dir.y > 0)
                return MoveDir.Up;
            else
                return MoveDir.Down;
        }

        public virtual void OnDamaged(GameObject attacker, int damage)
        {
            if (Room == null)
                return;

            Stat.Hp = Math.Max(Stat.Hp - damage, 0); //Hp가 0이하로 안내려가게함

            S_ChangeHp changePacket = new S_ChangeHp();
            changePacket.ObjectId = Id;
            changePacket.Hp = Stat.Hp;
            Room.Broadcast(changePacket);

            if(Stat.Hp <= 0)
            {
                Stat.Hp = 0;
                OnDead(attacker);
            }
        }
        public virtual void OnDead(GameObject attacker)
        {
            if (Room == null)
                return;

            S_Die diePacket = new S_Die();
            diePacket.ObjectId = Id;
            diePacket.AttackerId = attacker.Id;
            Room.Broadcast(diePacket);

            GameRoom room = Room;
            room.LeaveGame(Id);

            Stat.Hp = Stat.MaxHp;
            PosInfo.State = CreatureState.Idle;
            PosInfo.MoveDir = MoveDir.Down;
            PosInfo.PosX = 0;
            PosInfo.PosY = 0;

            room.EnterGame(this);
        }
    }
}
