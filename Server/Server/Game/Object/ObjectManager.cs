using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf.Protocol;

namespace Server.Game
{
    public class ObjectManager
    {
        public static ObjectManager Instance { get; } = new ObjectManager();

        object _lock = new object();
        Dictionary<int, Player> _players = new Dictionary<int, Player>();

        // [UNUSED(1)][TYPE(7)][ID(24)] -> ID생성규칙(int 32비트를 쪼개서 각 정보 표시 및 아이디 부여)
        int _counter = 0;

        public T Add<T>() where T : GameObject, new()
        {
            T gameObject = new T();

            lock(_lock)
            {
                gameObject.Id = GenerateId(gameObject.ObjectType); //만들어준 아이디
                
                if(gameObject.ObjectType == GameObjectType.Player) //현재 추가한 타입이 플레이어라면
                {                                        //현재 T타입이므로 Player타입으로 캐스팅 하여 넣어줌 (~~as~~)
                    _players.Add(gameObject.Id, gameObject as Player); //플레이어 딕셔너리에 추가
                }
            }

            return gameObject;
        }

        int GenerateId(GameObjectType type)
        {
            lock(_lock)
            {   //ID 생성규칙에 따라 아이디 생성 (type을 id24bit만큼 왼쪽으로 밀어줌 + counter(오브젝트 수)로 나머지 24비트 채워줌)
                return ((int)type << 24) | (_counter++); //아이디 생성 후 카운터(오브젝트 수 ) 하나 증가
            }
        }
        
        public static GameObjectType GetObjectTypeById(int id)
        {
            int type = (id >> 24) & 0x7F; // 아이디앞에 붙은 타입 추출
            return (GameObjectType)type;
        }

        public bool Remove(int objectId)
        {
            GameObjectType objectType = GetObjectTypeById(objectId); //타입을 가져와서

            lock (_lock)
            {
                if(objectType == GameObjectType.Player) //타입이 플레이어라면
                    return _players.Remove(objectId); //플레이어 제거
            }
            return false;
        }
        public Player Find(int objectId)
        {
            GameObjectType objectType = GetObjectTypeById(objectId);

            lock (_lock)
            {
                if (objectType == GameObjectType.Player)
                {
                    Player player = null;
                    if (_players.TryGetValue(objectId, out player))
                        return player;
                }
                return null;
            }
        }
    }
}
