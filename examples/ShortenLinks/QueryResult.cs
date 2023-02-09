using System.ComponentModel.DataAnnotations.Schema;

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