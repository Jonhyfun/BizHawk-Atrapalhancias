using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BizHawk.Client.EmuHawk.Jotas.Utils
{
	public class Queues
	{
		private static Dictionary<string, List<Thread>> queueList = new Dictionary<string, List<Thread>>();
		private static Dictionary<string, List<Task>> queueTaskList = new Dictionary<string, List<Task>>();
		public static Dictionary<string, Thread> executionThread = new Dictionary<string, Thread>();

		private static Task HandleQueue(List<Task> currentQueue)
		{
			currentQueue[0].RunSynchronously();
			currentQueue.RemoveAt(0);
			if (currentQueue.Count != 0)
			{
				var task = HandleQueue(currentQueue);
				if (!task.IsCompleted) task.RunSynchronously();
			}
			return Task.CompletedTask;
		}

		public static void DirectQueue(string queueName, Task queuedAction, int limit = 50)
		{
			if (!queueTaskList.ContainsKey(queueName)) queueTaskList.Add(queueName, new List<Task>());

			var currentQueue = queueTaskList[queueName];

			if (currentQueue.Count < limit)
			{
				currentQueue.Add(queuedAction);
			}

			if (!executionThread.ContainsKey(queueName))
			{
				executionThread.Add(queueName, new Thread(async () =>
				{
					await HandleQueue(currentQueue);
				}));
			}

			if (executionThread.ContainsKey(queueName) && executionThread[queueName].ThreadState != ThreadState.Running)
			{
				new Task(() =>
				{
					try
					{
						executionThread[queueName].Start();
						executionThread[queueName].Join();
						executionThread.Remove(queueName);
					}
					catch
					{

					}
				}).Start();
			}

		}

		public static void DebouncedQueue(string queueName, Action debouncedAction)
		{
			if (!queueList.ContainsKey(queueName)) queueList.Add(queueName, new List<Thread>());
			var currentQueue = queueList[queueName];

			if (currentQueue.Count != 0)
			{
				currentQueue[0].Interrupt();
				currentQueue.RemoveAt(0);
			}
			currentQueue.Add(new Thread(() =>
			{
				try
				{
					debouncedAction();
				}
				catch
				{

				}
			}));
			currentQueue[0].Start();
		}
	}
}
