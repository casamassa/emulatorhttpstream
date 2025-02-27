using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace EmulatorWebApp.Controllers
{
    [ApiController]
    [Route("/")]
    public class StreamController : ControllerBase
    {
        private static ConcurrentQueue<string> payloadQueue = new ConcurrentQueue<string>();

        [HttpGet("stream")]
        public async Task StreamPayloads(CancellationToken cancellationToken)
        {
            Response.ContentType = "text/event-stream";

            while (!cancellationToken.IsCancellationRequested)
            {
                if (payloadQueue.TryDequeue(out string payload))
                {
                    await Response.WriteAsync($"{payload}\n");
                    await Response.Body.FlushAsync();
                }
                await Task.Delay(500, cancellationToken);
            }
        }

        [HttpPost]
        [Route("senddata")]
        public IActionResult SendData([FromBody] JsonElement payload)
        {
            if (payload.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in payload.EnumerateArray())
                {
                    string payloadString = item.ToString().Replace("\r", "").Replace("\n", "");
                    payloadQueue.Enqueue(payloadString);
                    Console.WriteLine($"Payload recebido: {payloadString}");
                }
            }
            else
            {
                string payloadString = payload.ToString().Replace("\r", "").Replace("\n", "");
                payloadQueue.Enqueue(payloadString);
                Console.WriteLine($"Payload recebido: {payloadString}");
            }

            return Ok();
        }

        [HttpGet]
        [Route("ping")]
        public IActionResult Ping()
        {
            return Ok("pong"); 
        }
    }
}

