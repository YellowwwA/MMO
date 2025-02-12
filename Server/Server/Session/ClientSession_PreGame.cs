using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.DB;
using Server.Game;
using ServerCore;

namespace Server
{
    public partial class ClientSession : PacketSession
    {
		public int AccountDbId { get; private set; }
		public List<LobbyPlayerInfo> LobbyPlayers { get; set; } = new List<LobbyPlayerInfo>(); //현재 로비플레이어가 들고있는 목록 가져옴
        public void HandleLogin(C_Login loginPacket)
        {
			//TODO :보안체크
			if (ServerState != PlayerServerState.ServerStateLogin)
				return;
			//
			LobbyPlayers.Clear(); //HandleLogin이 두번실행되지 않도록

			using (AppDbContext db = new AppDbContext())
			{
				//로그인 시도한 아이디와 같은 아이디가 db에 있다면(기존에 만들어둔 아이디라면) 해당 아이디 리턴, 없다면 null 리턴
				AccountDb findAccount = db.Accounts
					.Include(a=>a.Players)
					.Where(a => a.AccountName == loginPacket.UniqueId).FirstOrDefault();

				if (findAccount != null) //기존 아이디를 찾았다면
				{
					//AccountDbId 메모리에 기억
					AccountDbId = findAccount.AccountDbId;

					S_Login loginOk = new S_Login() { LoginOk = 1 };
					foreach (PlayerDb playerDb in findAccount.Players)
                    {
						LobbyPlayerInfo lobbyPlayer = new LobbyPlayerInfo()
						{
							PlayerDbId = playerDb.PlayerDbId,
							Name = playerDb.PlayerName,
							StatInfo = new StatInfo()
							{
								Level = playerDb.Level,
								Hp = playerDb.Hp,
								MaxHp = playerDb.MaxHp,
								Attack = playerDb.Attack,
								Speed = playerDb.Speed,
								TotalExp = playerDb.TotalExp
							}
						};

						//메모리에도 들고 있기 (필요할때마다 다시 db에 접근해 정보를 가지고 오는건 낭비, 미리 처음에 접근해서 메모리에도 들고있고 필요할때 메모리에서 체크)
						LobbyPlayers.Add(lobbyPlayer);
						//패킷에 넣어준다
						loginOk.Players.Add(lobbyPlayer);
                    }

					Send(loginOk); //로그인 성공 패킷 전송, 접속가능
					//로비로 이동
					ServerState = PlayerServerState.ServerStateLobby;

				}
				else //가입안한아이디라면 (처음 접속했다면)
				{   //새로운 아이디 생성
					AccountDb newAccount = new AccountDb() { AccountName = loginPacket.UniqueId };
					db.Accounts.Add(newAccount);
					bool success = db.SaveChangesEx(); //만든 아이디 db에 저장
					if (success == false)
						return;

					//AccountDbId 메모리에 기억
					AccountDbId = newAccount.AccountDbId;

					S_Login loginOk = new S_Login() { LoginOk = 1 };
					Send(loginOk); //로그인 성공 패킷 전송, 접속가능
								   
					//로비로 이동
					ServerState = PlayerServerState.ServerStateLobby;
				}


			}
		}
		
		public void HandleEnterGame(C_EnterGame enterGamePacket)
        {
			if (ServerState != PlayerServerState.ServerStateLobby) //로비상태가 아닌데 접속을시도했다면 차단(리턴)
				return;

			//접속 요청한 플레이어의 이름과 메모리에 들고있는 플레이어의 이름과 같은게 있다면
			LobbyPlayerInfo playerInfo = LobbyPlayers.Find(p => p.Name == enterGamePacket.Name);
			if (playerInfo == null) // 이름이 없다면
				return;

			MyPlayer = ObjectManager.Instance.Add<Player>();
			{
				MyPlayer.PlayerDbId = playerInfo.PlayerDbId;
				MyPlayer.Info.Name = playerInfo.Name;
				MyPlayer.Info.PosInfo.State = CreatureState.Idle;
				MyPlayer.Info.PosInfo.MoveDir = MoveDir.Down;
				MyPlayer.Info.PosInfo.PosX = 0;
				MyPlayer.Info.PosInfo.PosY = 0;
				MyPlayer.Stat.MergeFrom(playerInfo.StatInfo);

				MyPlayer.Session = this;
			}

			ServerState = PlayerServerState.ServerStateGame;

			//TODO : 입장 요청 들어오면
			GameRoom room = RoomManager.Instance.Find(1);
			room.Push(room.EnterGame, MyPlayer);
		}
		public void HandleCreatePlayer(C_CreatePlayer createPacket)
        {
			//TODO :보안체크
			if (ServerState != PlayerServerState.ServerStateLogin)
				return;

			using (AppDbContext db = new AppDbContext())
            {
				PlayerDb findPlayer = db.Players
					.Where(p => p.PlayerName == createPacket.Name).FirstOrDefault();

				if(findPlayer != null)
                {
					//이름이겹친다면
					Send(new S_CreatePlayer());
                }
				else
                {
					//1레벨 스탯 정보 추출
					StatInfo stat = null;
					DataManager.StatDict.TryGetValue(1, out stat);

					//DB에 플레이어 생성해줌
					PlayerDb newPlayerDb = new PlayerDb()
					{
						PlayerName = createPacket.Name,
						Level = stat.Level,
						Hp = stat.Hp,
						MaxHp = stat.MaxHp,
						Attack = stat.Attack,
						Speed = stat.Speed,
						TotalExp = 0,
						AccountDbId = AccountDbId
					};

					db.Players.Add(newPlayerDb);
					bool success = db.SaveChangesEx(); // TODO: ExceptionHandling
					if (success == false)
						return;

					//메모리에 추가
					LobbyPlayerInfo lobbyPlayer = new LobbyPlayerInfo()
					{
						PlayerDbId = newPlayerDb.PlayerDbId,
						Name = createPacket.Name,
						StatInfo = new StatInfo()
						{
							Level = stat.Level,
							Hp = stat.Hp,
							MaxHp = stat.MaxHp,
							Attack = stat.Attack,
							Speed = stat.Speed,
							TotalExp = 0
						}
					};

					//메모리에도 들고 있기 (필요할때마다 다시 db에 접근해 정보를 가지고 오는건 낭비, 미리 처음에 접근해서 메모리에도 들고있고 필요할때 메모리에서 체크)
					LobbyPlayers.Add(lobbyPlayer);

					// 플레이어 생성 성공 클라에 전송
					S_CreatePlayer newPlayer = new S_CreatePlayer() { Player = new LobbyPlayerInfo() };
					newPlayer.Player.MergeFrom(lobbyPlayer);

					Send(newPlayer);
				}
            }
		}
	}
}
