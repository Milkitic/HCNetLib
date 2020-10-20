using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HCNetLib.Stream.Builder;

namespace HCNetLib.Stream
{
    public class AutoStreamManagement : StreamManagement, IDisposable
    {
        private readonly TimeSpan _overtime;

        private ConcurrentDictionary<StreamTask, DateTime> _heartBeatDictionary =
            new ConcurrentDictionary<StreamTask, DateTime>();

        private Task _autoManagementTask;
        private CancellationTokenSource _autoManagementCts;

        public AutoStreamManagement(string baseDir, string username, string password, TimeSpan overtime) :
            base(baseDir, username, password)
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
                            await keyValuePair.Key.StopAsync();
                            ConsoleHelper.WriteWarn("Auto dispose task: " + keyValuePair.Key.Module, "auto_management");
                        }
                    }

                    Thread.Sleep(1000);
                }
            });
        }

        public async Task<StreamTask> AddTaskWithHeartBeat(string host, int channel, BitStream bitStream, Size convertResolution)
        {
            var task = await base.AddTask(host, channel, bitStream, convertResolution);
            if (_heartBeatDictionary.ContainsKey(task))
                return _heartBeatDictionary.Keys.First(k => k.Equals(task));
            if (_heartBeatDictionary.TryAdd(task, DateTime.Now))
            {
                task.ProcessExit += StreamTask_ProcessExit;
            }
            else
            {
                throw new Exception("add failed");
            }

            return task;
        }

        public void HeartBeat(string host, int channel, BitStream bitStream)
        {
            var streamTask = new StreamTask(host, channel, bitStream, BaseDir, this);
            if (_heartBeatDictionary.ContainsKey(streamTask))
            {
                _heartBeatDictionary[streamTask] = DateTime.Now;
            }
        }

        private void StreamTask_ProcessExit(StreamTask obj)
        {
            _heartBeatDictionary.Remove(obj, out _);
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