using System.Data;

namespace Yandex.Ydb.Driver;

internal sealed record YdbConnectionState(ConnectionState ConnectionState, YdbConnectionSettings Settings);