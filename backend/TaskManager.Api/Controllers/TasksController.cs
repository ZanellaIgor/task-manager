using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.Common.Pagination;
using TaskManager.Application.DTOs.Tasks;
using TaskManager.Application.Filters;
using TaskManager.Application.Services.Interfaces;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/tasks")]
[Produces("application/json")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    /// <summary>
    /// Lista tarefas com filtros, paginação e ordenação.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedResult<TaskDto>>> GetAll([FromQuery] TaskFilterDto filters, CancellationToken cancellationToken)
    {
        var tasks = await _taskService.GetAllAsync(filters, cancellationToken);
        return Ok(tasks);
    }

    /// <summary>
    /// Retorna indicadores e listas resumidas para o dashboard.
    /// </summary>
    [HttpGet("overview")]
    [ProducesResponseType(typeof(TaskOverviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TaskOverviewDto>> GetOverview(CancellationToken cancellationToken)
    {
        var overview = await _taskService.GetOverviewAsync(cancellationToken);
        return Ok(overview);
    }

    /// <summary>
    /// Busca uma tarefa pelo identificador.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TaskDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var task = await _taskService.GetByIdAsync(id, cancellationToken);
        return Ok(task);
    }

    /// <summary>
    /// Cria uma nova tarefa.
    /// </summary>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TaskDto>> Create([FromBody] CreateTaskDto dto, CancellationToken cancellationToken)
    {
        var task = await _taskService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
    }

    /// <summary>
    /// Atualiza os dados de uma tarefa existente.
    /// </summary>
    [HttpPut("{id:int}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TaskDto>> Update(int id, [FromBody] UpdateTaskDto dto, CancellationToken cancellationToken)
    {
        var task = await _taskService.UpdateAsync(id, dto, cancellationToken);
        return Ok(task);
    }

    /// <summary>
    /// Marca uma tarefa como concluída.
    /// </summary>
    [HttpPatch("{id:int}/complete")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TaskDto>> Complete(int id, CancellationToken cancellationToken)
    {
        var task = await _taskService.CompleteAsync(id, cancellationToken);
        return Ok(task);
    }

    /// <summary>
    /// Cancela uma tarefa que ainda não foi concluída.
    /// </summary>
    [HttpPatch("{id:int}/cancel")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TaskDto>> Cancel(int id, CancellationToken cancellationToken)
    {
        var task = await _taskService.CancelAsync(id, cancellationToken);
        return Ok(task);
    }

    /// <summary>
    /// Remove uma tarefa que ainda não foi concluída.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _taskService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
