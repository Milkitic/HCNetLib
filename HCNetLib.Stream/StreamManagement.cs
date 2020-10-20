using System;
using HCNetLib.Stream.Builder;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HCNetLib.Stream
{
    public class StreamManagement : IDisposable
    {
        protected readonly string Username;
        protected readonly string Password;

        public StreamManagement(string baseDir, string username, string password)
        {
            Username = username;
            Password = password;
            BaseDir = baseDir;
        }

        public string BaseDir { get; set; }
        public HashSet<StreamTask> StreamTasks { get; set; } = new HashSet<StreamTask>();

        public async Task<StreamTask> AddTask(string host, int channel, BitStream bitStream, Size convertResolution)
        {
            var streamTask = new StreamTask(host, channel, bitStream, BaseDir, this);

            if (StreamTasks.TryGetValue(streamTask, out var task))
            {
                if (task.IsRunning) return task;
                streamTask = task;
            }
            else
            {
                StreamTasks.Add(streamTask);
            }

            await streamTask.RunAsync(Username, Password, convertResolution);
            streamTask.ProcessExit += StreamTask_ProcessExit;
            return streamTask;
        }

        public async Task<StreamTask> RemoveTask(string host, int channel, BitStream bitStream)
        {
            var streamTask = new StreamTask(host, channel, bitStream, BaseDir, this);

            if (StreamTasks.TryGetValue(streamTask, out var task))
            {
                await task.StopAsync();
                return task;
            }
            else
            {
                ConsoleHelper.WriteWarn("No available task found.", "management");
                return null;
            }
        }

        private void StreamTask_ProcessExit(StreamTask obj)
        {
            StreamTasks.Remove(obj);
        }

        public virtual void Dispose()
        {
            foreach (var streamTask in StreamTasks.ToList())
            {
                streamTask.StopAsync().Wait();
            }
        }
    }
}
