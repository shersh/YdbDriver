using Yandex.Ydb.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSingleton<ITaskRepository, YdbTaskRepository>();
builder.Services.AddYdbDataSource("Host=localhost;Port=2136;Pooling=true;MaxSessions=100;");

builder.Services.AddControllers();
builder.Services.AddHostedService<TaskWorker>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.MapControllers();

app.MapPost("/createSchema", async context =>
{
    await using var connection = context.RequestServices.GetRequiredService<YdbConnection>();
    var ydbCommand = connection.CreateYdbCommand();
    ydbCommand.CommandText = @"
CREATE TABLE tasks
(
    task_id String,
    created_at Uint64,
    task String,
    processing_at Uint64,
    is_processed bool,

PRIMARY KEY (task_id)
)

";

    await ydbCommand.ExecuteNonQueryAsync();
});

app.Run();