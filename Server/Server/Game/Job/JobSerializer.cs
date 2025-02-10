using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class JobSerializer
    {
		JobTimer _timer = new JobTimer();
		Queue<IJob> _jobQueue = new Queue<IJob>();
		object _lock = new object();
		bool _flush = false;

		public void PushAfter(int tickAfter, Action action) { PushAfter(tickAfter, new Job(action)); }
		public void PushAfter<T1>(int tickAfter, Action<T1> action, T1 t1) { PushAfter(tickAfter, new Job<T1>(action, t1)); }
		public void PushAfter<T1, T2>(int tickAfter, Action<T1, T2> action, T1 t1, T2 t2) { PushAfter(tickAfter, new Job<T1, T2>(action, t1, t2)); }
		public void PushAfter<T1, T2, T3>(int tickAfter, Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3) { PushAfter(tickAfter, new Job<T1, T2, T3>(action, t1, t2, t3)); }
		public void PushAfter(int tickAfter, IJob job) // 예약 시스템 (예약해놓고 나중에 사용할때)
        {
			_timer.Push(job, tickAfter);
        }

		public void Push(Action action) { Push(new Job(action)); }
		public void Push<T1>(Action<T1> action, T1 t1) { Push(new Job<T1>(action, t1)); }
		public void Push<T1,T2>(Action<T1, T2> action, T1 t1, T2 t2) { Push(new Job<T1, T2>(action, t1, t2)); }
		public void Push<T1,T2,T3>(Action<T1, T2,T3> action, T1 t1, T2 t2, T3 t3) { Push(new Job<T1, T2, T3>(action, t1, t2, t3)); }

		public void Push(IJob job) //당장사용해야할때
		{
			bool flush = false;

			lock (_lock)
			{
				_jobQueue.Enqueue(job);
				if (_flush == false) //만약 내가 처음으로 들어온거라면 실행까지 내가 함, ex가게에 들어갔더니 주방장이 없다면 내가 주방장을 맡아서 하고 이후 들어오는 애들은 주문서만 주방장에게 전해주고감
					flush = _flush = true;
			}

			if (flush)
				Flush();
		}

		public void Flush() //일감들 실행
		{
			_timer.Flush();

			while (true)
			{
				IJob job = Pop();
				if (job == null)
					return;

				job.Execute();
			}
		}

		IJob Pop()
		{
			lock (_lock)
			{
				if (_jobQueue.Count == 0) //더이상 실행할 일감이 없다면
				{
					_flush = false;
					return null;
				}
				return _jobQueue.Dequeue();
			}
		}
	}
}
