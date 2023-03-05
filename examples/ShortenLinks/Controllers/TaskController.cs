using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ShortenLinks.Controllers;

public record TaskToWork(string MethodName, string[] Arguments);

[Route("api/[controller]")]
[ApiController]
public class TaskController : ControllerBase
{
    private readonly ILogger<TaskController> _log;
    private readonly ITaskRepository _repository;

    public TaskController(ITaskRepository repository, ILogger<TaskController> log)
    {
        _repository = repository;
        _log = log;
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create()
    {
        try
        {
            var start = Stopwatch.GetTimestamp();
            await _repository.AddTask(new TaskToWork("Console.WriteLine", new[] { Guid.NewGuid().ToString() }));
            return Ok(new { elapsed = Stopwatch.GetElapsedTime(start) });
        }
        catch (Exception e)
        {
            _log.LogError(e, "Failed to process creating task");
            return Problem(e.ToString());
        }
    }
}