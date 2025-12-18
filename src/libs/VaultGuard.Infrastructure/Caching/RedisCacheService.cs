using System.Text.Json;
using StackExchange.Redis;
using VaultGuard.Application.Common.Interfaces;

namespace VaultGuard.Infrastructure.Caching;

/// <summary>
/// Redis cache service with key tracking for safe prefix deletion and enforced TTL.
/// </summary>
public sealed class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _database;
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(30);

    public RedisCacheService(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _database = redis.GetDatabase();
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        var value = await _database.StringGetAsync(key);

        if (!value.HasValue)
            return null;

        return JsonSerializer.Deserialize<T>(value.ToString());
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        var actualExpiration = expiration ?? DefaultExpiration;
        var serialized = JsonSerializer.Serialize(value);
        
        // Set the value with expiration
        await _database.StringSetAsync(key, serialized, actualExpiration, when: When.Always);

        // Track the key in a set based on its prefix for efficient deletion
        var prefix = ExtractPrefix(key);
        if (!string.IsNullOrEmpty(prefix))
        {
            var trackingSetKey = GetTrackingSetKey(prefix);
            await _database.SetAddAsync(trackingSetKey, key);
            // Set expiration on tracking set slightly longer than cache entries
            await _database.KeyExpireAsync(trackingSetKey, actualExpiration.Add(TimeSpan.FromMinutes(5)));
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _database.KeyDeleteAsync(key);
        
        // Remove from tracking set
        var prefix = ExtractPrefix(key);
        if (!string.IsNullOrEmpty(prefix))
        {
            var trackingSetKey = GetTrackingSetKey(prefix);
            await _database.SetRemoveAsync(trackingSetKey, key);
        }
    }

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        var trackingSetKey = GetTrackingSetKey(prefix);
        
        // Get all keys from the tracking set
        var trackedKeys = await _database.SetMembersAsync(trackingSetKey);
        
        if (trackedKeys.Length > 0)
        {
            // Delete all tracked keys
            var keysToDelete = trackedKeys.Select(k => (RedisKey)k.ToString()).ToArray();
            await _database.KeyDeleteAsync(keysToDelete);
        }
        
        // Delete the tracking set itself
        await _database.KeyDeleteAsync(trackingSetKey);
    }

    /// <summary>
    /// Extracts prefix from a cache key (assumes format "prefix:suffix" or "prefix:part1:part2").
    /// Returns the first segment before the first colon.
    /// </summary>
    private static string ExtractPrefix(string key)
    {
        var colonIndex = key.IndexOf(':');
        return colonIndex > 0 ? key[..colonIndex] : key;
    }

    /// <summary>
    /// Gets the Redis key for the tracking set of a given prefix.
    /// </summary>
    private static string GetTrackingSetKey(string prefix)
    {
        return $"_tracking:{prefix}";
    }
}
