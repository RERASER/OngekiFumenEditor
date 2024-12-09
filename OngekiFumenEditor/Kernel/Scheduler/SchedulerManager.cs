using Caliburn.Micro;
using OngekiFumenEditor.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OngekiFumenEditor.Kernel.Scheduler
{
	[Export(typeof(ISchedulerManager))]
	[PartCreationPolicy(CreationPolicy.Shared)]
	internal class SchedulerManager : ISchedulerManager
	{
		private AbortableThread runThread;

		private List<ISchedulable> schedulers { get; } = new List<ISchedulable>();

		private ConcurrentDictionary<ISchedulable, DateTime> schedulersCallTime { get; } = new();

		public IEnumerable<ISchedulable> Schedulers => schedulers;

		public async Task Init()
		{
			foreach (var s in IoC.GetAll<ISchedulable>())
			{
				await AddScheduler(s);
			}

			runThread = new(Run)
			{
				Name = "SchedulerManager::Run()"
			};
			runThread.Start();

			return;
		}

		public Task AddScheduler(ISchedulable s)
		{
			if (s is null || schedulers.FirstOrDefault(x => x.SchedulerName.Equals(s.SchedulerName)) != null)
			{
				Log.LogWarn($"Can't add scheduler : {s?.SchedulerName} is null/exist.");
				return Task.CompletedTask;
			}

			schedulers.Add(s);
			schedulersCallTime[s] = DateTime.MinValue;
			Log.LogDebug("Added new scheduler: " + s.SchedulerName);
			return Task.CompletedTask;
		}

		private void Run(CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				var schedulers = Schedulers
					.Where(x => x is not null && DateTime.UtcNow - schedulersCallTime[x] >= x.ScheduleCallLoopInterval)//DateTime.Now有性能问题
					.Select(x => x.OnScheduleCall(cancellationToken).ContinueWith(_ => schedulersCallTime[x] = DateTime.UtcNow))
					.ToArray();
				if (schedulers.Length > 0)
					Task.WaitAll(schedulers, cancellationToken);
				else
					Thread.Sleep(10);
			}
		}

		public async Task Term()
		{
			Log.LogDebug("call SchedulerManager.Dispose()");

			try
			{
				runThread.Abort();
			}
			catch { }

			foreach (var scheduler in Schedulers)
			{
				Log.LogInfo("Call OnSchedulerTerm() :" + scheduler.SchedulerName);
				scheduler.OnSchedulerTerm();
				await Task.Yield();
			}
		}

		public async Task RemoveScheduler(ISchedulable s)
		{
			await Task.Yield();
			if (s is null || schedulers.FirstOrDefault(x => x.SchedulerName.Equals(s.SchedulerName)) is null)
			{
				Log.LogWarn($"Can't remove scheduler : {s?.SchedulerName} is null or not exist.");
				return;
			}

			schedulers.Remove(s);
			Log.LogDebug("Remove scheduler: " + s.SchedulerName);
		}
	}
}
