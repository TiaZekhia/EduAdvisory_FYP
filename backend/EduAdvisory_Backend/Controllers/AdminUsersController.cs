using EduAdvisory_Backend.DTOs.Admin;
using EduAdvisory_Backend.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduAdvisory_Backend.Controllers;

[Authorize(Roles = "ADMIN")]
[ApiController]
[Route("api/admin/users")]
public class AdminUsersController : ControllerBase
{
    private readonly IAdminUserManagementService _adminUserManagementService;

    public AdminUsersController(IAdminUserManagementService adminUserManagementService)
    {
        _adminUserManagementService = adminUserManagementService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers(CancellationToken ct)
    {
        var users = await _adminUserManagementService.GetUsersAsync(ct);
        return Ok(users);
    }

    [HttpGet("available-links")]
    public async Task<IActionResult> GetAvailableLinks([FromQuery] string role, CancellationToken ct)
    {
        try
        {
            var links = await _adminUserManagementService.GetAvailableLinksAsync(role, ct);
            return Ok(links);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateManagedUserDto dto, CancellationToken ct)
    {
        try
        {
            var createdUser = await _adminUserManagementService.CreateUserAsync(dto, ct);
            return Ok(createdUser);
        }
        catch (InvalidOperationException ex)
        {
            return BuildExpectedErrorResponse(ex.Message);
        }
    }

    [HttpPut("{userId:int}")]
    public async Task<IActionResult> UpdateUser(int userId, [FromBody] UpdateManagedUserDto dto, CancellationToken ct)
    {
        try
        {
            var updatedUser = await _adminUserManagementService.UpdateUserAsync(userId, dto, ct);
            return Ok(updatedUser);
        }
        catch (InvalidOperationException ex)
        {
            return BuildExpectedErrorResponse(ex.Message);
        }
    }

    [HttpPut("{userId:int}/deactivate")]
    public async Task<IActionResult> DeactivateUser(int userId, CancellationToken ct)
    {
        try
        {
            var updatedUser = await _adminUserManagementService.DeactivateUserAsync(userId, ct);
            return Ok(updatedUser);
        }
        catch (InvalidOperationException ex)
        {
            return BuildExpectedErrorResponse(ex.Message);
        }
    }

    [HttpPut("{userId:int}/reactivate")]
    public async Task<IActionResult> ReactivateUser(int userId, CancellationToken ct)
    {
        try
        {
            var updatedUser = await _adminUserManagementService.ReactivateUserAsync(userId, ct);
            return Ok(updatedUser);
        }
        catch (InvalidOperationException ex)
        {
            return BuildExpectedErrorResponse(ex.Message);
        }
    }

    private IActionResult BuildExpectedErrorResponse(string message)
    {
        if (message.Contains("already exists", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("already used", StringComparison.OrdinalIgnoreCase))
        {
            return Conflict(message);
        }

        if (message.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(message);
        }

        return BadRequest(message);
    }
}