using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Text.Json;
using Dapper;
using Npgsql;
using NpgsqlTypes;
using ShortenLinks.Controllers;
using Yandex.Ydb.Driver;
using Ydb.Sdk;
using Ydb.Sdk.Auth;
using Ydb.Sdk.Table;
using Ydb.Sdk.Value;


public interface ITaskRepository
{
    Task AddTask(TaskToWork entity);

    Task<QueryResult?> GetNextTask();

    Task SetAsProcessing(string taskId);
}

public class YdbNativeRepository : ITaskRepository
{
    private readonly Driver _driver;

    public YdbNativeRepository(ILoggerFactory loggerFactory)
    {
        var config = new DriverConfig(
            endpoint: "grpc://localhost:2136", // Database endpoint, "grpcs://host:port"
            database: "/local", // Full database path
            credentials: new AnonymousProvider() // Credentials provider, see "Credentials" section
        );

        var driver = new Driver(
            config: config,
            loggerFactory: loggerFactory
        );

        _driver = driver;

        driver.Initialize().GetAwaiter().GetResult();
    }

    public async Task AddTask(TaskToWork entity)
    {
        using var tableClient = new TableClient(_driver, new TableClientConfig());
        var response = await tableClient.SessionExec(async session =>
        {
            var query = @"DECLARE $task as String;
DECLARE $id as String;
DECLARE $created_at as Uint64;  

UPSERT INTO tasks (`task_id`, `task`, `created_at`) VALUES ($id, $task, $created_at)";

            return await session.ExecuteDataQuery(
                query: query,
                parameters: new Dictionary<string, YdbValue>
                {
                    { "$id", YdbValue.MakeString(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString())) },
                    { "$task", YdbValue.MakeString(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(entity))) },
                    { "$created_at", YdbValue.MakeUint64((ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()) },
                },
                // Begin serializable transaction and commit automatically after query execution
                txControl: TxControl.BeginSerializableRW().Commit()
            );
        });
        
        response.Status.EnsureSuccess();
    }

    public async Task<QueryResult?> GetNextTask()
    {
        return null;
    }

    public Task SetAsProcessing(string taskId)
    {
        throw new NotImplementedException();
    }
}

public class NpgsqlRepository : ITaskRepository
{
    private readonly NpgsqlDataSource _source;

    public NpgsqlRepository(NpgsqlDataSource source)
    {
        _source = source;
    }

    public async Task AddTask(TaskToWork entity)
    {
        await using var connection = await _source.OpenConnectionAsync();
        var command = connection.CreateCommand();
        command.CommandText = @"
insert into tasks (task_id, task, is_processed, processing_at, created_at)
values (@id, @task, false, null, @created);";
        command.Parameters.AddWithValue("id", Guid.NewGuid().ToString());
        command.Parameters.AddWithValue("task", JsonSerializer.Serialize(entity));
        command.Parameters.AddWithValue("created", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

        await command.ExecuteNonQueryAsync();
    }

    public async Task<QueryResult?> GetNextTask()
    {
        await using var connection = await _source.OpenConnectionAsync();
        var first = await connection.QueryFirstOrDefaultAsync<QueryResult>(
            "SELECT * FROM tasks WHERE processing_at IS NULL ORDER BY created_at LIMIT 1;");

        return first ?? null;
    }

    public async Task SetAsProcessing(string taskId)
    {
        await using var connection = _source.CreateConnection();
        var dynamicParameters = new DynamicParameters();
        dynamicParameters.Add("id", taskId);
        await connection.ExecuteAsync(
            "UPDATE tasks SET processing_at = 123 WHERE task_id = @id",
            dynamicParameters);
    }
}

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
        // var dynamicParameters = new DynamicParameters();
        // dynamicParameters.Add("$id", taskId);
        // await connection.ExecuteAsync(
        //     "DECLARE $id as String; UPDATE tasks SET processing_at = 123 WHERE task_id = $id",
        //     dynamicParameters);
    }
}

public class QueryResult
{
    public QueryResult()
    {
    }

    public QueryResult(string TaskId, ulong CreatedAt, bool? IsProcessed, string Task)
    {
        this.TaskId = TaskId;
        this.CreatedAt = CreatedAt;
        this.IsProcessed = IsProcessed;
        this.Task = Task;
    }

    [Column("task_id")] public string TaskId { get; init; }
    [Column("created_at")] public ulong CreatedAt { get; init; }
    [Column("is_processed")] public bool? IsProcessed { get; init; }
    [Column("task")] public string Task { get; init; }

    public void Deconstruct(out string TaskId, out ulong CreatedAt, out bool? IsProcessed, out string Task)
    {
        TaskId = this.TaskId;
        CreatedAt = this.CreatedAt;
        IsProcessed = this.IsProcessed;
        Task = this.Task;
    }
}


public class TaskWorker : IHostedService
{
    private readonly ILogger<TaskWorker> _log;
    private readonly ITaskRepository _taskRepository;
    private Task _task;
    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();


    public TaskWorker(ILogger<TaskWorker> log, ITaskRepository taskRepository)
    {
        _log = log;
        _taskRepository = taskRepository;

        Dapper.SqlMapper.SetTypeMap(
            typeof(QueryResult),
            new CustomPropertyTypeMap(
                typeof(QueryResult),
                (type, columnName) =>
                    type.GetProperties().FirstOrDefault(prop =>
                        prop.GetCustomAttributes(false)
                            .OfType<ColumnAttribute>()
                            .Any(attr => attr.Name == columnName))));
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _task = Task.Factory.StartNew(Processing, cancellationToken);
        return Task.CompletedTask;
    }

    private async Task Processing()
    {
        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            await Task.Delay(1000);

            try
            {
                var nextTask = await _taskRepository.GetNextTask();
                if (nextTask == null)
                    continue;

                await _taskRepository.SetAsProcessing(nextTask.TaskId);
            }
            catch (Exception e)
            {
                _log.LogError(e, "Failed to process task");
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource.Cancel();
        return Task.CompletedTask;
    }
}