using HCNetLib;
using HCNetLib.Stream;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Logging;

namespace HCNetStreamServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StreamController : ControllerBase
    {
        private readonly AutoStreamManagement _autoStreamManagement;

        public StreamController(AutoStreamManagement autoStreamManagement)
        {
            _autoStreamManagement = autoStreamManagement;
        }

        [HttpGet("{host}/{port}/{channel}/{bitStream}/get")]
        [AllowAnonymous]
        public async Task<IActionResult> Get(string username, string password, string host,
            int channel, BitStream bitStream, int port = 554, int width = 480, int height = 270)
        {
            var rtspIdentity = new RtspIdentity(host, port, channel, bitStream);
            _autoStreamManagement.HeartBeat(rtspIdentity);
            if (!_autoStreamManagement.StreamTasks.TryGetValue(rtspIdentity, out var task) || !task.IsRunning)
            {
                task = await _autoStreamManagement.AddTaskWithHeartBeat(rtspIdentity, new Size(width, height),
                    new RtspAuthentication(username, password));

                ConsoleHelper.WriteInfo("Waiting to generate files...", "req @ " + rtspIdentity);
                using (var cts = new CancellationTokenSource(3000))
                {
                    while (!System.IO.File.Exists(task.FilePath))
                    {
                        cts.Token.ThrowIfCancellationRequested();
                    }
                }

                ConsoleHelper.WriteInfo("File generated", "req @ " + rtspIdentity);
            }
            else
            {
                await task.WaitForReading();
            }

            var myFile = await System.IO.File.ReadAllBytesAsync(task.FilePath);
            return File(myFile, "application/x-mpegURL", Path.GetFileName(task.FilePath));
        }

        [HttpGet("{host}/{port}/{channel}/{bitStream}/{fileName}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFile(string host, int channel, BitStream bitStream, int port, string fileName)
        {
            var rtspIdentity = new RtspIdentity(host, port, channel, bitStream);
            if (!_autoStreamManagement.StreamTasks.ContainsKey(rtspIdentity))
                return BadRequest();
            var task = _autoStreamManagement.StreamTasks[rtspIdentity];

            var filePath = Path.Combine(Path.GetDirectoryName(task.FilePath) ?? string.Empty, fileName);
            var myFile = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(myFile, "video/vnd.dlna.mpeg-tts", Path.GetFileName(filePath));
        }

        //[HttpGet]
        //[Authorize("Permission")]
        //public async Task<ActionResult<IEnumerable<string>>> Get()
        //{
        //}
    }
}
