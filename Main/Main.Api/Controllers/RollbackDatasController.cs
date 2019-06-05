using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Main.Domain.Concrete;
using Main.Repository.EntityFramework;
using Shared.Repository.Core;

namespace Main.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RollbackDatasController : ControllerBase
    {
        private readonly EntityRepository _context;
        private readonly IGenericRepository _handler;

        public RollbackDatasController(EntityRepository context)
        {
            _context = context;
            _handler = new GenericEntityRepositoryHandler(_context);
        }

        // GET: api/RollbackDatas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RollbackData>>> GetRollbackDatas(RollbackData data)
        {
            try
            {
                var results = await Task.Run(async () => _handler.FindMultiple(data));

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

            //return await _context.RollbackDatas.ToListAsync();
        }

        // GET: api/RollbackDatas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RollbackData>> GetRollbackData(int id)
        {
            if (id == -1)
            {
                _context.Database.EnsureDeleted();
                _context.Database.EnsureCreated();

                return Accepted();
            }
            else
            {
                return Conflict(new NotSupportedException());

                //var rollbackData = await _context.RollbackDatas.FindAsync(id);

                //if (rollbackData == null)
                //{
                //    return NotFound();
                //}

                //return rollbackData; 
            }
        }

        // PUT: api/RollbackDatas/5
        [HttpPut/*("{id}")*/]
        public async Task<IActionResult> PutRollbackData(RollbackData data)
        {
            try
            {
                var result = _handler.Update(data);

                if (result)
                {
                    await _context.SaveChangesAsync();
                    return StatusCode(StatusCodes.Status202Accepted,
                        string.Format("Succesfully updated RollbackData, Result -> {0}", result));
                }
                else
                {
                    return StatusCode(StatusCodes.Status409Conflict, string.Format("Unable to update RollbackData, Result -> {0}", result));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            //if (id != rollbackData.Id)
            //{
            //    return BadRequest();
            //}

            //_context.Entry(rollbackData).State = EntityState.Modified;

            //try
            //{
            //    await _context.SaveChangesAsync();
            //}
            //catch (DbUpdateConcurrencyException)
            //{
            //    if (!RollbackDataExists(id))
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

        // POST: api/RollbackDatas
        [HttpPost]
        public async Task<ActionResult<RollbackData>> PostRollbackData(RollbackData data)
        {
            try
            {
                var result = _handler.Add(data);

                if (result)
                {
                    await _context.SaveChangesAsync();
                    return StatusCode(StatusCodes.Status201Created,
                        string.Format("Succesfully added RollbackData, Result -> {0}", result));
                }
                else
                {
                    return StatusCode(StatusCodes.Status409Conflict, string.Format("Unable to add RollbackData, Result -> {0}", result));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw e;
            }

            //_context.RollbackDatas.Add(rollbackData);
            //await _context.SaveChangesAsync();

            //return CreatedAtAction("GetRollbackData", new { id = rollbackData.Id }, rollbackData);
        }

        // DELETE: api/RollbackDatas/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<RollbackData>> DeleteRollbackData(int id)
        {
            var rollbackData = await _context.RollbackDatas.FindAsync(id);
            if (rollbackData == null)
            {
                return NotFound();
            }

            _context.RollbackDatas.Remove(rollbackData);
            await _context.SaveChangesAsync();

            return rollbackData;
        }

        private bool RollbackDataExists(int id)
        {
            return _context.RollbackDatas.Any(e => e.Id == id);
        }
    }
}
