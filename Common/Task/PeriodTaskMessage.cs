using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;

namespace bfbd.Common.Tasks
{
	/// <summary>
	/// 周期性任务（消息处理）
	/// </summary>
	/// <remarks>2014-11-24</remarks>
	partial class PeriodTask
	{
		#region Task Message

		private static ConcurrentQueue<TaskMessage> _messages = new ConcurrentQueue<TaskMessage>();

		public static void PostMessage(string taskName, object param = null, int waitSeconds = 0)
		{
			var msg = new TaskMessage()
			{
				Name = taskName,
				Param = param,
				Start = DateTime.Now,
				Wait = waitSeconds
			};
			_messages.Enqueue(msg);
		}

		private void ProcessMessage()
		{
			while (_messages.Count > 0)
			{
				TaskMessage msg;
				if (_messages.TryDequeue(out msg))
				{
					if (msg.Start.AddSeconds(msg.Wait) < DateTime.Now)
						continue;

					var task = _tasks.FirstOrDefault(t => t.Name == msg.Name);
					if (task != null)
					{
						ExecuteTask(task, msg.Param == null ? task.State : msg.Param);
					}
				}
			}
		}

		#region TaskMessage

		class TaskMessage
		{
			public string Name;		// 任务名称
			public object Param;	// 任务参数
			public DateTime Start;	// 添加时间
			public int Wait;		// 等待秒数
		}

		#endregion

		#endregion
	}
}
