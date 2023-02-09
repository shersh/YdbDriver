using ShortenLinks.Controllers;

public interface ITaskRepository
{
    Task AddTask(TaskToWork entity);

    Task<QueryResult?> GetNextTask();

    Task SetAsProcessing(string taskId);
}