using Microsoft.AspNetCore.Mvc;
using SuperSee.DTOs;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System;

namespace SuperSee.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private readonly AppDbContext _db;

    public StudentsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public ActionResult<IEnumerable<StudentDto>> GetAllStudents()
    {
        var students = _db.Students.ToList();
        var dtos = students.Select(s => new StudentDto
        {
            StudentId = s.StudentId,
            StudentName = s.StudentName,
            StudentEmail = s.StudentEmail
        });
        return Ok(dtos);
    }

    [HttpGet("tasks")]
    public ActionResult<IEnumerable<TaskDto>> GetStudentTasks([FromQuery] Guid studentId)
    {
        try
        {
            var student = _db.Students
                .FirstOrDefault(s => s.StudentId == studentId);

            if (student == null)
                return NotFound(new { error = "Student not found" });

            if (student.TeamId == null)
                return Ok(new List<TaskDto>());

            // Get all tasks for this student's team
            var tasks = _db.Tasks
                .Where(t => t.TeamId == student.TeamId)
                .Include(t => t.Milestones)
                .OrderByDescending(t => t.CreatedAt)
                .ToList();

            var dtos = tasks.Select(t => MapToTaskDto(t)).ToList();
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    private TaskDto MapToTaskDto(SuperSee.Task task)
    {
        return new TaskDto
        {
            TaskId = task.TaskId,
            Title = task.Title,
            Description = task.Description,
            Progress = task.Progress,
            CreatedAt = task.CreatedAt,
            DueDate = task.DueDate,
            IsCompleted = task.IsCompleted,
            Milestones = task.Milestones?.Select(m => new MilestoneDto
            {
                MilestoneId = m.MilestoneId,
                Title = m.Title,
                IsCompleted = m.IsCompleted,
                DueDate = m.DueDate,
                Order = m.Order
            }).OrderBy(m => m.Order).ToList() ?? new()
        };
    }
}
