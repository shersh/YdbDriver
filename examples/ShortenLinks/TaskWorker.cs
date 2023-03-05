using System.ComponentModel.DataAnnotations.Schema;
using Dapper;

public class TaskWorker : IHostedService
{
    private readonly ILogger<TaskWorker> _log;
    private readonly ITaskRepository _taskRepository;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private Task _task;


    public TaskWorker(ILogger<TaskWorker> log, ITaskRepository taskRepository)
    {
        _log = log;
        _taskRepository = taskRepository;

        SqlMapper.SetTypeMap(
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

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource.Cancel();
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
}