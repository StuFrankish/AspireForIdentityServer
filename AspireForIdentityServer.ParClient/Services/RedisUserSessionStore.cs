using Duende.Bff;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;

namespace Client.Services;

public class RedisUserSessionStore(IDistributedCache cache, IConnectionMultiplexer connectionMultiplexer) : IUserSessionStore
{
    private readonly IDistributedCache _cache = cache;
    private readonly IConnectionMultiplexer _connectionMultiplexer = connectionMultiplexer;

    private readonly DistributedCacheEntryOptions _defaultCacheOptions = new()
    {
        SlidingExpiration = TimeSpan.FromHours(6)
    };

    private static string UserSessionCacheKey(string userSession) => $"user-session:{userSession}";
    private static string UserStorageCacheKey(string subjectId, string customKey) => $"user-storage:{subjectId}:{customKey}";

    public void SetString(string SubjectId, string key, string value)
    {
        var cacheKey = UserStorageCacheKey(SubjectId, key);
        _cache.SetString(cacheKey, value, _defaultCacheOptions);
    }

    public string GetString(string SubjectId, string key)
    {
        var cacheKey = UserStorageCacheKey(SubjectId, key);
        return _cache.GetString(cacheKey);
    }

    public void SetObject<T>(string SubjectId, string key, T value)
    {
        var cacheKey = UserStorageCacheKey(SubjectId, key);
        var valueJson = JsonSerializer.Serialize(value);
        _cache.SetString(cacheKey, valueJson, _defaultCacheOptions);
    }

    public T GetObject<T>(string SubjectId, string key)
    {
        var cacheKey = UserStorageCacheKey(SubjectId, key);
        var value = _cache.GetString(cacheKey);
        return value != null ? JsonSerializer.Deserialize<T>(value) : default;
    }

    public IEnumerable<string> GetUserStorageKeys(string subjectId)
    {
        var server = _connectionMultiplexer.GetServer(_connectionMultiplexer.GetEndPoints()[0]);
        var allKeys = server.Keys(pattern: $"user-storage:{subjectId}:*").ToList();
        return allKeys.Select(k => k.ToString().Split(":").Last());
    }

    public async Task CreateUserSessionAsync(UserSession session, CancellationToken cancellationToken = default)
    {
        var sessionJson = JsonSerializer.Serialize(session);
        await _cache.SetStringAsync(UserSessionCacheKey(session.Key), sessionJson, _defaultCacheOptions, cancellationToken);
    }

    public async Task<UserSession> GetUserSessionAsync(string key, CancellationToken cancellationToken = default)
    {
        var sessionJson = await _cache.GetStringAsync(UserSessionCacheKey(key), cancellationToken);
        return sessionJson != null ? JsonSerializer.Deserialize<UserSession>(sessionJson) : null;
    }

    public async Task UpdateUserSessionAsync(string key, UserSessionUpdate sessionUpdate, CancellationToken cancellationToken = default)
    {
        var sessionJson = await _cache.GetStringAsync(UserSessionCacheKey(key), cancellationToken);
        if (sessionJson is not null)
        {
            var session = JsonSerializer.Deserialize<UserSession>(sessionJson);
            if (session is not null)
            {
                sessionUpdate.CopyTo(session);

                var updatedSessionJson = JsonSerializer.Serialize(session);
                await _cache.SetStringAsync(UserSessionCacheKey(key), updatedSessionJson, cancellationToken);
            }
        }
    }

    public async Task DeleteUserSessionAsync(string key, CancellationToken cancellationToken = default)
    {
        await _cache.RemoveAsync(UserSessionCacheKey(key), cancellationToken);
    }

    public async Task<IReadOnlyCollection<UserSession>> GetUserSessionsAsync(UserSessionsFilter filter, CancellationToken cancellationToken = default)
    {
        filter.Validate();

        var server = _connectionMultiplexer.GetServer(_connectionMultiplexer.GetEndPoints()[0]);
        var allKeys = (server.Keys(pattern: "user-session:*") ?? []).ToList();
        var sessions = new HashSet<UserSession>();

        foreach (var item in allKeys)
        {
            var i = await _cache.GetStringAsync(item.ToString(), cancellationToken);
            var session = i is null ? null : JsonSerializer.Deserialize<UserSession>(i);

            if (session is not null &&
                (
                    (!string.IsNullOrWhiteSpace(filter.SessionId) && filter.SessionId == session.SessionId) ||
                    (!string.IsNullOrWhiteSpace(filter.SubjectId) && filter.SubjectId == session.SubjectId)
                )
            )
            {
                sessions.Add(session);
            }
        }
        return sessions;
    }

    public async Task DeleteUserSessionsAsync(UserSessionsFilter filter, CancellationToken cancellationToken = default)
    {
        filter.Validate();

        var keysToBeDeleted = await GetUserSessionsAsync(filter, cancellationToken);
        foreach (var key in keysToBeDeleted.Select(s => s.Key).ToList())
        {
            await DeleteUserSessionAsync(key, cancellationToken);
        }
    }
}