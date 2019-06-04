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
    public class EventsController : ControllerBase
    {
        private readonly EntityRepository _context;
        private readonly IGenericRepository _handler;

        public EventsController(EntityRepository context)
        {
            _context = context;
            _handler = new GenericEntityRepositoryHandler(_context);
        }

        // GET: api/Events
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Event>>> GetEvents(Event @event)
        {
            try
            {
                var results = await Task.Run(async () => _handler.FindMultiple(@event));

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

            //return await _context.Events.ToListAsync();
            
        }

        // GET: api/Events/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Event>> GetEvent(int id)
        {
            if (id == -1)
            {
                _context.Database.EnsureDeleted();
                _context.Database.EnsureCreated();

                return Accepted();
            }
            else
            {
                var @event = await _context.Events.FindAsync(id);

                if (@event == null)
                {
                    return NotFound();
                }

                return @event;
            }
        }

        // PUT: api/Events/5
        [HttpPut/*("{id}")*/]
        public async Task<IActionResult> PutEvent(Event @event)
        {
            try
            {
                var result = _handler.Update(@event);

                if (result)
                {
                    await _context.SaveChangesAsync();
                    return StatusCode(StatusCodes.Status202Accepted,
                        string.Format("Succesfully updated Event, Result -> {0}", result));
                }
                else
                {
                    return StatusCode(StatusCodes.Status409Conflict, string.Format("Unable to update Event, Result -> {0}", result));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            //if (id != @event.Id)
            //{
            //    return BadRequest();
            //}

            //_context.Entry(@event).State = EntityState.Modified;

            //try
            //{
            //    await _context.SaveChangesAsync();
            //}
            //catch (DbUpdateConcurrencyException)
            //{
            //    if (!EventExists(id))
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

        // POST: api/Events
        [HttpPost]
        public async Task<ActionResult<Event>> PostEvent(Event @event)
        {
            try
            {
                var result = _handler.Add(@event);

                if (result)
                {
                    await _context.SaveChangesAsync();
                    return StatusCode(StatusCodes.Status201Created,
                        string.Format("Succesfully added Event, Result -> {0}", result));
                }
                else
                {
                    return StatusCode(StatusCodes.Status409Conflict, string.Format("Unable to add Event, Result -> {0}", result));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw e;
            }

            //_context.Events.Add(@event);
            //await _context.SaveChangesAsync();

            //return CreatedAtAction("GetEvent", new { id = @event.Id }, @event);
        }

        // DELETE: api/Events/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Event>> DeleteEvent(int id)
        {
            var @event = await _context.Events.FindAsync(id);
            if (@event == null)
            {
                return NotFound();
            }

            _context.Events.Remove(@event);
            await _context.SaveChangesAsync();

            return @event;
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.Id == id);
        }
    }
}
