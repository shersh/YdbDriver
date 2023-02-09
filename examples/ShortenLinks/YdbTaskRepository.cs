using System.Text.Json;
using ShortenLinks.Controllers;
using Yandex.Ydb.Driver;

public class YdbTaskRepository : ITaskRepository
{
    private readonly YdbDataSource _source;

    public YdbTaskRepository(YdbDataSource dataSource)
    {
        _source = dataSource;
    }

    public async Task AddTask(TaskToWork entity)
    {
        await using var connection = await _source.OpenConnectionAsync();

        var ydbCommand = connection.CreateYdbCommand();
        ydbCommand.AddParameter("$id", Guid.NewGuid().ToString());
        ydbCommand.AddParameter("$task", JsonSerializer.Serialize(entity));
        ydbCommand.AddParameter("$created_at", (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

        ydbCommand.CommandText = @"
DECLARE $task as String;
DECLARE $id as String;
DECLARE $created_at as Uint64;  

UPSERT INTO tasks (`task_id`, `task`, `created_at`) VALUES ($id, $task, $created_at)";

        await ydbCommand.ExecuteNonQueryAsync();
    }

    public async Task<QueryResult?> GetNextTask()
    {
        return null;
        
    
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

        // var first = await connection.QueryFirstOrDefaultAsync<QueryResult>(
        //     "SELECT * FROM tasks WHERE processing_at IS NULL ORDER BY created_at LIMIT 1;");

        return first ?? null;
    }

    public async Task SetAsProcessing(string taskId)
    {
        await using var connection = await _source.OpenConnectionAsync();
        var ydbCommand = connection.CreateYdbCommand();
        ydbCommand.CommandText = @"DECLARE $id as String; UPDATE tasks SET processing_at = 123 WHERE task_id = $id";
        await ydbCommand.ExecuteNonQueryAsync();
    }
}