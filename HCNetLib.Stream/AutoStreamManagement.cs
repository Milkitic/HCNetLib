using HCNetLib.Stream.Builder;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HCNetLib.Stream
{
    public class AutoStreamManagement : StreamManagement
    {
        private readonly TimeSpan _overtime;

        private ConcurrentDictionary<RtspIdentity, DateTime> _heartBeatDictionary =
            new ConcurrentDictionary<RtspIdentity, DateTime>();

        private Task _autoManagementTask;
        private CancellationTokenSource _autoManagementCts;

        public AutoStreamManagement(string baseDir, TimeSpan overtime, string defaultUsername = null,
            string defaultPassword = null) : base(baseDir, defaultUsername, defaultPassword)
        {
            _overtime = overtime;
            _autoManagementCts = new CancellationTokenSource();
            _autoManagementTask = Task.Factory.StartNew(async () =>
            {
                while (!_autoManagementCts.IsCancellationRequested)
                {
                    var allDic = _heartBeatDictionary.ToList();
                    foreach (var keyValuePair in allDic)
                    {
                        if (DateTime.Now - keyValuePair.Value > _overtime)
                        {
                            var streamTask = StreamTasks[keyValuePair.Key];
                            await streamTask.StopAsync();
                            _heartBeatDictionary.TryRemove(keyValuePair.Key, out _);
                            ConsoleHelper.WriteWarn("Auto dispose task: " + streamTask.Module, "auto_management");
                        }
                    }

                    Thread.Sleep(1000);
                }
            }, TaskCreationOptions.LongRunning);
        }

        public async Task<StreamTask> AddTaskWithHeartBeat(RtspIdentity rtspIdentity, Size convertResolution, RtspAuthentication authentication = null)
        {
            var task = await base.AddTask(rtspIdentity, convertResolution, authentication);

            if (_heartBeatDictionary.TryAdd(rtspIdentity, DateTime.Now))
            {
                task.ProcessExit += StreamTask_ProcessExit;
            }
            else
            {
                if (!_heartBeatDictionary.ContainsKey(rtspIdentity))
                    ConsoleHelper.WriteWarn("Add auto dispose task failed", "auto_management");
            }

            return task;
        }

        public void HeartBeat(RtspIdentity rtspIdentity)
        {
            if (_heartBeatDictionary.TryGetValue(rtspIdentity, out var old))
            {
                var now = DateTime.Now;
                _heartBeatDictionary[rtspIdentity] = now;
                //ConsoleHelper.WriteInfo(
                //    "Update task `" + rtspIdentity + "` expire time from " + old.ToLongTimeString() + " to " +
                //    now.ToLongTimeString(), "auto_management");
            }
            else
            {
                ConsoleHelper.WriteWarn("HeartBeat error: cannot find task `" + rtspIdentity + "`", "auto_management");
            }
        }

        private void StreamTask_ProcessExit(StreamTask obj)
        {
            _heartBeatDictionary.TryRemove(obj.Identity, out _);
        }

        public override void Dispose()
        {
            base.Dispose();
            _autoManagementCts.Cancel();
            _autoManagementCts?.Dispose();
            _autoManagementTask?.Wait();
            _autoManagementTask?.Dispose();
        }
    }
}