﻿using Grpc.Core;
using Yandex.Ydb.Driver.Internal.TypeHandling;
using Ydb.Table;
using Ydb.Table.V1;

namespace Yandex.Ydb.Driver;

public sealed class SessionPool : ISessionPool
{
    private readonly YdbConnector _connector;
    private readonly int _maxSessions;
    private readonly object _lck = new object();

    private readonly Queue<string> _idle = new Queue<string>();
    private readonly Dictionary<string, Session> _sessions;

    internal SessionPool(YdbConnector connector, int maxSessions)
    {
        _connector = connector;
        _maxSessions = maxSessions;
        semaphoreSlim = new SemaphoreSlim(maxSessions);
        _sessions = new Dictionary<string, Session>(maxSessions);
    }

    private SemaphoreSlim semaphoreSlim;

    async Task ISessionPool.Initialize(string database)
    {
        for (int i = 0; i < _maxSessions; i++)
        {
            var response = await _connector.UnaryCallAsync(TableService.CreateSessionMethod, createRequest,
                new CallOptions(new Metadata()
                {
                    { YdbMetadata.RpcDatabaseHeader, database },
                }));

            lock (_lck)
            {
                var result = response.Operation.GetResult<CreateSessionResult>();
                var session = new Session(result.SessionId, database);
                _sessions.Add(result.SessionId, session);
                _idle.Enqueue(session.Id);
            }
        }
    }

    async ValueTask<string> ISessionPool.GetSession(string database)
    {
        await semaphoreSlim.WaitAsync();

        lock (_lck)
        {
            if (_idle.TryDequeue(out var session))
            {
                return session;
            }
        }

        throw new YdbDriverException("No idle session");
    }

    private CreateSessionRequest createRequest = new CreateSessionRequest();

    void ISessionPool.Return(string sessionId)
    {
        lock (_lck)
        {
            _idle.Enqueue(sessionId);
        }

        semaphoreSlim.Release();
    }

    public void Dispose()
    {
        lock (_lck)
        {
            foreach (var (key, value) in _sessions)
            {
                var response = _connector.UnaryCall(TableService.DeleteSessionMethod,
                    new DeleteSessionRequest() { SessionId = key },
                    new CallOptions(new Metadata() { { YdbMetadata.RpcDatabaseHeader, value.Database } }) { });
            }
        }

        semaphoreSlim.Dispose();
    }
}