using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.Diagnostics;

namespace bfbd.Common.Tasks
{
    /// <summary>
    /// 周期性任务
    /// </summary>
    /// <remarks>2015/1/21</remarks>
    public partial class PeriodTask : PeriodTaskBase
	{
		#region Task Implement

		private DateTime _startTime = DateTime.Now;

		/// <summary>
		/// 创建周期性任务
		/// </summary>
		/// <param name="pulse">循环周期（毫秒）</param>
		public PeriodTask(int pulse)
		{
			Pulse = pulse;
			Wait = 0;
		}

		public override void Start()
		{
			_startTime = DateTime.Now;
			base.Start();
		}

		protected override void Execute()
		{
			ProcessMessage();

			foreach (TaskInfo task in _tasks)
			{
				if (task.Interval == 0)
					continue;
				if (DateTime.Now < _startTime.AddSeconds(task.Wait))
					continue;
				if (DateTime.Now < task.LastEndTime.AddSeconds(task.Interval))
					continue;

				ExecuteTask(task, task.State);
			}
		}

		private void ExecuteTask(TaskInfo task, object param)
		{
			Debug.Assert(task != null);
			try
			{
				task.Action(param);
			}
			catch (Exception ex)
			{
				Logger.Exception(ex);
				if (OnException != null)
				{
					try { OnException(this, new TaskExceptionArgs(task.Name, param, ex)); }
					catch (Exception) { }
				}
			}
			finally
			{
				task.LastEndTime = DateTime.Now;
			}
		}

		public event EventHandler<TaskExceptionArgs> OnException;

		#endregion Task Implement
	}

	partial class PeriodTask
	{
		#region Task Init

		private ConcurrentBag<TaskInfo> _tasks = new ConcurrentBag<TaskInfo>();

		/// <summary>
		/// 添加新任务
		/// </summary>
		/// <param name="name">任务名称</param>
		/// <param name="action">任务操作</param>
		/// <param name="state">任务参数</param>
		/// <param name="intervalSeconds">间隔时间</param>
		/// <param name="waitSeconds">等待时间</param>
		public void AddTask(string name, Action<object> action, object state, int intervalSeconds, int waitSeconds)
		{
			if (_tasks.Any(t => t.Name == name))
				throw new ArgumentException("Task name has existed.");

			_tasks.Add(new TaskInfo()
			{
				Name = name,
				Action = action,
				State = state,
				Wait = waitSeconds,
				Interval = intervalSeconds,
				LastEndTime = DateTime.MinValue
			});
		}

		public void AddTask<T>(string name, Action<T> action, T state, int intervalSeconds, int waitSeconds)
			where T : class
		{
			if (_tasks.Any(t => t.Name == name))
				throw new ArgumentException("Task name has existed.");

			_tasks.Add(new TaskInfo()
			{
				Name = name,
				Action = o=>action(o as T),
				State = state,
				Wait = waitSeconds,
				Interval = intervalSeconds,
				LastEndTime = DateTime.MinValue
			});
		}

		#region TaskInfo

		class TaskInfo
		{
			public string Name;
			public Action<object> Action;
			public object State;
			public int Wait;
			public int Interval;
			public DateTime LastEndTime;
		}

		#endregion

		#endregion
	}

	#region Task Exception

	public class TaskExceptionArgs : EventArgs
	{
		public string TaskName { get; private set;}
		public object State { get; private set; }
		public Exception Exception { get; private set; }

		public TaskExceptionArgs(string name, object state, Exception ex)
		{
			TaskName = name; State = state; Exception = ex;
		}
	}

	#endregion Task Exception
}
