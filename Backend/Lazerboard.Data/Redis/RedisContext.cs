using StackExchange.Redis;

namespace Lazerboard.Data.Redis;


public class RedisContext(string connectionString)
{
    public ConnectionMultiplexer ConnectionMultiplexer = ConnectionMultiplexer.Connect(connectionString);
}