using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Google.Protobuf.WellKnownTypes;
using Server.Data;
using Server.DB;
using ServerCore;

namespace Server.Game
{
    class Program
	{
		static Listener _listener = new Listener();
		static List<System.Timers.Timer> _timers = new List<System.Timers.Timer>();

		static void TickRoom(GameRoom room, int tick = 100) //TickRoom을 이용해 일 분배가 가능해짐(전에는 메인 스레드만 계속 돌리는데 TickRoom을 사용하여 다른스레드들에도 일 배정)
        {
			var timer = new System.Timers.Timer();
			timer.Interval = tick; //실행주기 설정 tick ms 마다 실행
			timer.Elapsed += ((s, e) => { room.Update(); }); //초가 다되면 이 함수를 실행해주겠다.
			timer.AutoReset = true; // 매번 초기화
			timer.Enabled = true;

			_timers.Add(timer);
        }
		static void Main(string[] args)
		{
			ConfigManager.LoadConfig();
			DataManager.LoadData();

			GameRoom room = RoomManager.Instance.Add(1);
			TickRoom(room, 50); //50ms마다 실행

			// DNS (Domain Name System)
			string host = Dns.GetHostName();
			IPHostEntry ipHost = Dns.GetHostEntry(host);
			IPAddress ipAddr = ipHost.AddressList[0];
			IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

			_listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
			Console.WriteLine("Listening...");

			//FlushRoom();
			//JobTimer.Instance.Push(FlushRoom);

			while (true)
			{
				DbTransaction.Instance.Flush();
			}
		}
	}
}
