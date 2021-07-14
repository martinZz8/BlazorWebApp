using BlazorWebApp.Server.Data;
using BlazorWebApp.Shared;
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
    public class UnitController : ControllerBase
    {
        private DataContext _context;
        public UnitController(DataContext context)
        {
            _context = context;
        }

        //Static data about Units is below
        /*
        public IList<Unit> Units => new List<Unit>
        {
            new Unit { Id=1, Title="Knight", Attack=10, Defense=10, BananaCost=100 },
            new Unit { Id=2, Title="Archer", Attack=15, Defense=5, BananaCost=150 },
            new Unit { Id=3, Title="Mage", Attack=20, Defense=1, BananaCost=200 },
        };
        */

        [HttpGet]
        public async Task<IActionResult> GetUnits()
        {
            var units = await _context.Units.ToListAsync();
            return Ok(units);
        }

        [HttpPost] //to many posts we have to add a unique route like: [HttpPost("AddUnit")]
        public async Task<IActionResult> AddUnit(Unit unit)
        {
            _context.Units.Add(unit);
            await _context.SaveChangesAsync();
            return Ok(await _context.Units.ToListAsync());
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUnit(int id, Unit unit)
        {
            var dbUnit = await _context.Units.FirstOrDefaultAsync(u => u.Id == id);
            if (dbUnit != null)
            {
                dbUnit.Title = unit.Title;
                dbUnit.Attack = unit.Attack;
                dbUnit.Defense = unit.Defense;
                dbUnit.BananaCost = unit.BananaCost;
                dbUnit.HitPoints = unit.HitPoints;
                await _context.SaveChangesAsync();
                return Ok(dbUnit);
            }
            else
            {
                return NotFound("Unit with the given Id doesn't exist.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUnit(int id)
        {
            var dbUnit = await _context.Units.FirstOrDefaultAsync(u => u.Id == id);
            if (dbUnit != null)
            {
                _context.Units.Remove(dbUnit);
                await _context.SaveChangesAsync();
                return Ok(await _context.Units.ToListAsync());
            }
            else
            {
                return NotFound("Unit with the given Id doesn't exist.");
            }
        }
    }
}
