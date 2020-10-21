using System;
using System.Collections.Concurrent;
using HCNetLib.Stream.Builder;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HCNetLib.Stream
{
    public class StreamManagement : IDisposable
    {
        protected readonly string DefaultUsername;
        protected readonly string DefaultPassword;

        public StreamManagement(string baseDir, string defaultUsername = null, string defaultPassword = null)
        {
            DefaultUsername = defaultUsername;
            DefaultPassword = defaultPassword;
            BaseDir = baseDir;
        }

        public string BaseDir { get; set; }

        public ConcurrentDictionary<RtspIdentity, StreamTask> StreamTasks { get; set; } =
            new ConcurrentDictionary<RtspIdentity, StreamTask>();

        public async Task<StreamTask> AddTask(RtspIdentity rtspIdentity, Size convertResolution,
            RtspAuthentication authentication)
        {
            if (StreamTasks.TryGetValue(rtspIdentity, out var streamTask))
            {
                if (streamTask.IsRunning) return streamTask;
            }
            else
            {
                streamTask = new StreamTask(rtspIdentity, this);
                while (!StreamTasks.TryAdd(rtspIdentity, streamTask))
                {
                }
            }

            if (!streamTask.IsRunning)
            {
                try
                {
                    await streamTask.RunAsync(DefaultUsername ?? authentication.Credential.UserName, DefaultPassword ?? authentication.Credential.Password, convertResolution);
                }
                catch (Exception ex)
                {
                    StreamTasks.TryRemove(rtspIdentity, out _);
                    throw;
                }

                streamTask.ProcessExit += StreamTask_ProcessExit;
            }

            return streamTask;
        }

        public async Task<StreamTask> RemoveTask(RtspIdentity rtspIdentity)
        {
            if (StreamTasks.TryGetValue(rtspIdentity, out var task))
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
            while (!StreamTasks.TryRemove(obj.Identity, out _))
            {
            }
        }

        public virtual void Dispose()
        {
            foreach (var streamTask in StreamTasks.Values.ToList())
            {
                streamTask.StopAsync().Wait();
            }
        }
    }
}
