using System;
using System.Threading;
using System.Threading.Tasks;

namespace OngekiFumenEditor.Utils
{
	public class AbortableThread
	{
		private CancellationTokenSource cancellationTokenSource;
		private Thread thread;

		public CancellationToken CancellationToken => cancellationTokenSource.Token;

		public AbortableThread(Action<CancellationToken> cancellableMethod)
		{
			cancellationTokenSource = new CancellationTokenSource();
			thread = new(() => { cancellableMethod?.Invoke(CancellationToken); })
			{
				IsBackground = true
			};
			Name = $"AbortableThread:{cancellableMethod}";
		}

		public string Name { get; set; }

		public void Start()
		{
			thread.Start();
			Log.LogDebug($"Thread {Name} started.", prefix: "AbortableThread");
		}

		public void Abort(bool waitForTask = true)
		{
			Log.LogDebug($"Begin to abort thread {Name}.", prefix: "AbortableThread");
			cancellationTokenSource.Cancel();
			if (waitForTask)
			{
				if (thread.ThreadState.HasFlag(ThreadState.Running))
				{
					thread.Join();
				}
			}
			Log.LogDebug($"Aborted thread {Name}.", prefix: "AbortableThread");
		}
	}

	public class AbortableTask
	{
		private CancellationTokenSource cancellationTokenSource;
		private Task? task;
		private Func<CancellationToken, Task> taskFactory;

		public CancellationToken CancellationToken => cancellationTokenSource.Token;

		public AbortableTask(Func<CancellationToken, Task> cancellableMethod)
		{
			cancellationTokenSource = new CancellationTokenSource();
			Name = $"AbortableTask:{cancellableMethod}";
			taskFactory = cancellableMethod;
		}

		public string Name { get; set; }

		public void Start()
		{
			if(task == null)
			{
				task = new(() => { taskFactory?.Invoke(CancellationToken); });
				if (task.Status <= TaskStatus.Created)
				{
					task.Start();
				}
				Log.LogDebug($"Task {Name} started.", prefix: "AbortableTask");
			}
		}

		public void Abort(bool waitForTask = true)
		{
			Log.LogDebug($"Begin to abort task {Name}.", prefix: "AbortableTask");
			cancellationTokenSource.Cancel();
			if (waitForTask)
				task?.Wait();
			Log.LogDebug($"Aborted task {Name}.", prefix: "AbortableTask");
		}

		public async Task AbortAsync()
		{
			Log.LogDebug($"Begin to abort task {Name}.", prefix: "AbortableTask");
			cancellationTokenSource.Cancel();
			if(task != null)
			{
				await task;
			}
			Log.LogDebug($"Aborted task {Name}.", prefix: "AbortableTask");
		}
	}
}
