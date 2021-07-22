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
    public class BattleController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IUtilityService _utilityService;

        public BattleController(DataContext context, IUtilityService utilityService)
        {
            _context = context;
            _utilityService = utilityService;
        }

        [HttpPost]
        public async Task<IActionResult> StartBattle([FromBody] int opponentId)
        {
            var attacker = await _utilityService.GetUser();
            var opponent = await _context.Users.FindAsync(opponentId);
            if(opponent == null || opponent.IsDeleted)
            {
                return NotFound("Opponent not available");
            }

            var result = new BattleResult();
            await Fight(attacker, opponent, result);

            return Ok(result);
        }

        private async Task Fight(User attacker, User opponent, BattleResult result)
        {
            var attackerArmy = await _context.UserUnits
                .Where(unit => unit.UserId == attacker.Id && unit.HitPoints > 0)
                .Include(u => u.Unit)
                .ToListAsync();

            var opponentArmy = await _context.UserUnits
                .Where(unit => unit.UserId == opponent.Id && unit.HitPoints > 0)
                .Include(u => u.Unit)
                .ToListAsync();

            var attackerDamageSum = 0;
            var opponentDamageSum = 0;

            int currentRound = 0;

            while(attackerArmy.Count > 0 && opponentArmy.Count > 0)
            {
                currentRound++;

                if (currentRound % 2 != 0)
                    attackerDamageSum += FightRound(attacker, opponent, attackerArmy, opponentArmy, result);
                else
                    opponentDamageSum += FightRound(opponent, attacker, opponentArmy, attackerArmy, result);
            }

            result.IsVictory = opponentArmy.Count == 0;
            result.RoundsFought = currentRound;

            if(result.RoundsFought > 0)            
                await FinishFight(attacker, opponent, result, attackerDamageSum, opponentDamageSum);
        }

        private int FightRound(User attacker, User opponent, List<UserUnit> attackerArmy, List<UserUnit> opponentArmy, BattleResult result)
        {
            int randomAttackerIndex = new Random().Next(attackerArmy.Count);
            int randomOpponentIndex = new Random().Next(opponentArmy.Count);

            var randomAttacker = attackerArmy[randomAttackerIndex];
            var randomOpponent = opponentArmy[randomOpponentIndex];

            var damage = new Random().Next(randomAttacker.Unit.Attack) - new Random().Next(randomOpponent.Unit.Defense);
            if (damage < 0)
                damage = 0;

            string resultInfo;

            if(damage < randomOpponent.HitPoints)
            {
                randomOpponent.HitPoints -= damage;
                resultInfo =
                    $"{attacker.Username}'s {randomAttacker.Unit.Title} attacks " +
                    $"{opponent.Username}'s {randomOpponent.Unit.Title} with {damage} damage.";
            }
            else
            {
                damage = randomOpponent.HitPoints;
                randomOpponent.HitPoints = 0;
                opponentArmy.Remove(randomOpponent);
                resultInfo =
                    $"{attacker.Username}'s {randomAttacker.Unit.Title} kills " +
                    $"{opponent.Username}'s {randomOpponent.Unit.Title} with {damage} damage!";
            }

            result.Log.Add(resultInfo);
            return damage;
        }

        private async Task FinishFight(User attacker, User opponent, BattleResult result, int attackerDamageSum, int opponentDamageSum)
        {
            result.AttacterDamageSum = attackerDamageSum;
            result.OpponentDamageSum = opponentDamageSum;

            attacker.Battles++;
            opponent.Battles++;

            if(result.IsVictory)
            {
                attacker.Victories++;
                opponent.Defeats++;
                attacker.Bananas += opponentDamageSum;
                opponent.Bananas += attackerDamageSum * 10;
            }
            else
            {
                attacker.Defeats++;
                opponent.Victories++;
                attacker.Bananas += opponentDamageSum * 10;
                opponent.Bananas += attackerDamageSum;
            }

            StoreBattleHistory(attacker, opponent, result);

            await _context.SaveChangesAsync();
        }

        private void StoreBattleHistory(User attacker, User opponent, BattleResult result)
        {
            var battle = new Battle
            {
                Attacker = attacker,
                Opponent = opponent,
                RoundsFought = result.RoundsFought,
                WinnerDamage = result.IsVictory ? result.AttacterDamageSum : result.OpponentDamageSum,
                Winner = result.IsVictory ? attacker : opponent
            };

            _context.Battles.Add(battle);
        }
    }
}
