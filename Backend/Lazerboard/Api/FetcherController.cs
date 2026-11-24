using System.Text;
using Lazerboard.Data.SharedEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Lazerboard.Api;

[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
[Route("api/[controller]")]
public class FetcherController: ControllerBase
{
    private readonly HttpClient _client;
    private readonly ILogger<FetcherController> _logger;
    private readonly ConnectionFactory _connectionFactory;
    
    private const string BaseUrl = "http://scorefetcher:8080";
    private const string RabbitMqHost = "http://rabbitmq:5672";
    
    public FetcherController(HttpClient client, ILogger<FetcherController> logger, IConfiguration config)
    {
        _client = client;
        _logger = logger;
        
        var rabbitMqConfiguration = config.GetSection("RabbitMQ");
        var rabbitUser = rabbitMqConfiguration["User"]!;
        var rabbitPassword = rabbitMqConfiguration["Password"]!;
        _connectionFactory = new ConnectionFactory { HostName = RabbitMqHost, UserName = rabbitUser, Password = rabbitPassword };
    }
    
    /// <summary>
    /// Send request to the score fetcher service
    /// </summary>
    /// <param name="method">The <see cref="HttpMethod"/></param>
    /// <param name="requestString">The request string</param>
    /// <param name="content">The <see cref="HttpContent"/> or null</param>
    /// <param name="ct">A <see cref="CancellationToken"/></param>
    /// <returns>The <see cref="HttpResponseMessage"/></returns>
    private async Task<HttpResponseMessage> SendRequestAsync(HttpMethod method, 
        string requestString,
        HttpContent? content,
        CancellationToken ct = default)
    {
        var requestMessage = new HttpRequestMessage(method, requestString);
        requestMessage.Content = content;
        
        var response = await _client.SendAsync(requestMessage, ct);
        
        return response;
    }

    [HttpGet("seedingstate")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSeedingStateAsync(CancellationToken ct)
    {
        var requestString = $"{BaseUrl}/seedingstate";

        try
        {
            var response = await SendRequestAsync(HttpMethod.Get, requestString, null, ct);
            return Ok(await response.Content.ReadAsStringAsync(ct));
        }
        catch (Exception)
        {
            return StatusCode(503, "Score fetcher service unavailable");
        }
    }

    /// <summary>
    /// Send task to recalculate beatmaps
    /// </summary>
    /// <param name="command">The <see cref="BeatmapQueryCommand"/></param>
    /// <param name="ct">A <see cref="CancellationToken"/></param>
    /// <returns>Whether sending a message to RabbitMQ was successful or not</returns>
    [HttpPost("recalculatebeatmaps")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> SendTaskToRecalculateMapsAsync([FromBody] BeatmapQueryCommand command, CancellationToken ct)
    {
        if (command.BeatmapIds.Length == 0 && command.BeatmapsetId == null)
        {
            return BadRequest("Nothing to recalculate!");
        }
        try
        {
            await using var connection = await _connectionFactory.CreateConnectionAsync(ct);
            await using var channel = await connection.CreateChannelAsync(cancellationToken: ct);

            await channel.QueueDeclareAsync(queue: "beatmaprecalcs", durable: true, exclusive: false, autoDelete: false,
                arguments: new Dictionary<string, object?> { { "x-queue-type", "quorum" } }, cancellationToken: ct);
            
            var encodedCommand = JsonConvert.SerializeObject(command);
            var commandBytes = Encoding.UTF8.GetBytes(encodedCommand);

            await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "beatmaprecalcs", body: commandBytes, cancellationToken: ct);
            _logger.Log(LogLevel.Information, "Sent command to recalc beatmaps: {@command}", command);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.Log(LogLevel.Warning, ex, "Failed sending the message to RabbitMQ");
            return StatusCode(503, "RabbitMQ is unavailable");
        }
    }
}