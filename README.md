# YDB.ADO 
YDB C# Driver for [YDB (Yandex Database)](https://github.com/ydb-platform/ydb). This driver implements YDB protobuf protocol and supports ADO.NET rules.

## Installation
[Get it on Nuget][https://www.nuget.org/packages/Yandex.Ydb.ADO/]
```bash
PM> Install-Package Yandex.Ydb.Ado
```
To get hosting extensions
```bash
PM> Install-Package Yandex.Ydb.DependencyInjection
```


## Features

- [x] Sync and Async API
- [x] Pooling sessions
- [x] Simple statements
- [x] Dependency Injection integration
- [x] Primitives mapping (Int32, Bool, DateTime, Guid etc.)
- [x] Json type mapping


## Basic usage

```csharp
// injecting YdbDataSource with connection string
builder.Services.AddYdbDataSource("Host=localhost;Port=2136;Pooling=true;MaxSessions=100;");
```

In controller class or services usage:

```csharp
    public YdbTaskRepository(YdbDataSource dataSource)
    {
        _source = dataSource;
    }
    ...
    
    public async Task AddTask(TaskToWork entity)
    {
        // Getting connection (it will not open physical connection and will get session from pool by default) 
        await using var connection = await _source.OpenConnectionAsync();

        // Creating command
        var ydbCommand = connection.CreateYdbCommand();
        ydbCommand.AddParameter("$id", Guid.NewGuid().ToString());
        ydbCommand.AddParameter("$task", JsonSerializer.Serialize(entity));
        ydbCommand.AddParameter("$created_at", (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

        ydbCommand.CommandText = @"
            DECLARE $task as String;
            DECLARE $id as String;
            DECLARE $created_at as Uint64;  
            
            UPSERT INTO tasks (`task_id`, `task`, `created_at`) VALUES ($id, $task, $created_at)";

        // Executing command
        await ydbCommand.ExecuteNonQueryAsync();
    }
    
    
    public async Task<QueryResult?> GetNextTask()
    {
        await using var connection = await _source.OpenConnectionAsync();
        var ydbCommand = connection.CreateYdbCommand();
        ydbCommand.CommandText =
            "SELECT task_id, task, created_at, is_processed FROM tasks WHERE processing_at IS NULL ORDER BY created_at LIMIT 1;";

        var reader = await ydbCommand.ExecuteReaderAsync();
        var resultAsync = await reader.NextResultAsync();
        if (!resultAsync)
            return null;

        var first = new QueryResult()
        {
            TaskId = reader.GetString(0),
            Task = reader.GetString(1),
            CreatedAt = reader.GetFieldValue<UInt64>(2),
            IsProcessed = reader.GetFieldValue<Boolean>(3),
        };

        return first ?? null;
    }
    
```



## Planned

- [ ] Prepared statements
- [ ] Batch statements
- [ ] Custom user defined mappers
- [ ] Streaming
- [ ] Flexible retry backoff policies

