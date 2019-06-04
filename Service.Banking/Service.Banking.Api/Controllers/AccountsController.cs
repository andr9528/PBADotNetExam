using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service.Banking.Domain.Concrete;
using Service.Banking.Repository.EntityFramework;
using Shared.Repository.Core;

namespace Service.Banking.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly EntityRepository _context;
        private readonly IGenericRepository _handler;

        public AccountsController(EntityRepository context)
        {
            _context = context;
            _handler = new GenericEntityRepositoryHandler(_context);
        }

        // GET: api/Accounts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Account>>> GetAccounts(Account account)
        {
            try
            {
                var results = await Task.Run(async () => _handler.FindMultiple(account));

                return Ok(results);
            }
            catch (Exception e)
            {
                if (e.Message.Contains("Found no results"))
                {
                    return NoContent();
                }
                else
                {
                    Console.WriteLine(e);
                    throw e;
                }
            }

            //return await _context.Accounts.ToListAsync();
        }

        // GET: api/Accounts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Account>> GetAccount(int id)
        {
            if (id == -1)
            {
                _context.Database.EnsureDeleted();
                _context.Database.EnsureCreated();

                return Accepted();
            }
            else
            {
                var account = await _context.Accounts.FindAsync(id);

                if (account == null)
                {
                    return NotFound();
                }

                return account;
            }
        }
        //[HttpPut]
        //public async Task<IActionResult> Transfer((string from, string to, double amount) input)
        //{
        //    try
        //    {
        //        var fromAccount = _handler.Find(new Account() {AccountNumber = input.from});
        //        var toAccount = _handler.Find(new Account() {AccountNumber = input.to });

        //        fromAccount.Balance = fromAccount.Balance - input.amount;
        //        toAccount.Balance = toAccount.Balance + input.amount;

        //        var fromAccountResult = _handler.Update(fromAccount);
        //        var toAccountResult = _handler.Update(toAccount);

        //        if (fromAccountResult & toAccountResult) 
        //        {
        //            await _context.SaveChangesAsync();
        //            return StatusCode(StatusCodes.Status202Accepted,
        //                string.Format("Succesfully transfered money between Accounts, FromResult -> {0}, ToResult -> {1}",
        //                    fromAccountResult, toAccountResult));
        //        }
        //        else
        //        {
        //            return StatusCode(StatusCodes.Status409Conflict,
        //                string.Format("Unable to transfered money between Accounts, FromResult -> {0}, ToResult -> {1}", fromAccountResult,
        //                    toAccountResult));
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e);
        //        throw;
        //    }
        //}

        // PUT: api/Accounts/
        [HttpPut/*("{id}")*/]
        public async Task<IActionResult> PutAccount(Account account)
        {
            try
            {
                var result = _handler.Update(account);

                if (result)
                {
                    await _context.SaveChangesAsync();
                    return StatusCode(StatusCodes.Status202Accepted,
                        string.Format("Succesfully updated Account, Result -> {0}", result));
                }
                else
                {
                    return StatusCode(StatusCodes.Status409Conflict, string.Format("Unable to update Account, Result -> {0}", result));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            //if (id != account.Id)
            //{
            //    return BadRequest();
            //}

            //_context.Entry(account).State = EntityState.Modified;

            //try
            //{
            //    await _context.SaveChangesAsync();
            //}
            //catch (DbUpdateConcurrencyException)
            //{
            //    if (!AccountExists(id))
            //    {
            //        return NotFound();
            //    }
            //    else
            //    {
            //        throw;
            //    }
            //}

            //return NoContent();
        }

        // POST: api/Accounts
        [HttpPost]
        public async Task<ActionResult<Account>> PostAccount(Account account)
        {
            try
            {
                var result = _handler.Add(account);

                if (result)
                {
                    await _context.SaveChangesAsync();
                    return StatusCode(StatusCodes.Status201Created,
                        string.Format("Succesfully added Account, Result -> {0}", result));
                }
                else
                {
                    return StatusCode(StatusCodes.Status409Conflict, string.Format("Unable to add Account, Result -> {0}", result));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw e;
            }

            //_context.Accounts.Add(account);
            //await _context.SaveChangesAsync();

            //return CreatedAtAction("GetAccount", new { id = account.Id }, account);
        }

        // DELETE: api/Accounts/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Account>> DeleteAccount(int id)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }

            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();

            return account;
        }

        private bool AccountExists(int id)
        {
            return _context.Accounts.Any(e => e.Id == id);
        }
    }
}
