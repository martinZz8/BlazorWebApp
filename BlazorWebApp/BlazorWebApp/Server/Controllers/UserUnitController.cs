using BlazorWebApp.Server.Data;
using BlazorWebApp.Server.Services;
using BlazorWebApp.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorWebApp.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserUnitController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IUtilityService _utilityService;

        public UserUnitController(DataContext context, IUtilityService utilityService)
        {
            _context = context;
            _utilityService = utilityService;
        }

        [HttpPost]
        public async Task<IActionResult> BuildUserUnit([FromBody] int unitId)
        {
            var unit = await _context.Units.FirstOrDefaultAsync<Unit>(u => u.Id == unitId);
            var user = await _utilityService.GetUser();
            if (user.Bananas >= unit.BananaCost)
            {
                user.Bananas -= unit.BananaCost;
                var newUserUnit = new UserUnit
                {
                    UnitId = unitId,
                    UserId = user.Id,
                    HitPoints = unit.HitPoints
                };

                _context.UserUnits.Add(newUserUnit);
                await _context.SaveChangesAsync();
                return Ok(newUserUnit);
            }
            else
            {
                return BadRequest("Not enought bananas!");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUserUnits()
        {
            var user = await _utilityService.GetUser();
            var userUnits = await _context.UserUnits.Where(unit => unit.UserId == user.Id).ToListAsync();

            var response = userUnits.Select(
                unit => new UserUnitResponse
                {
                    UnitId = unit.UnitId,
                    HitPoints = unit.HitPoints
                });

            return Ok(response);
        }
    }
}
