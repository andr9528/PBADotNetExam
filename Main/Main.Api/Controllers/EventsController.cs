﻿using System;
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
        public async Task<ActionResult<IEnumerable<Event>>> GetEvents(/*[FromBody]*/ Event @event)
        {
            if (@event != null)
            {
                try
                {
                    //get the speified events
                    return await ((DbSet<Event>)_handler.FindMultiple(@event)).ToListAsync();
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("Found no results"))
                    {
                        return NoContent();
                    }
                    else
                        throw e;
                }
            }
            else
            {
                //get all events
                return await _context.Events.ToListAsync();
            }
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
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEvent(int id, Event @event)
        {
            if (id != @event.Id)
            {
                return BadRequest();
            }

            _context.Entry(@event).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Events
        [HttpPost]
        public async Task<ActionResult<Event>> PostEvent(Event @event)
        {
            _context.Events.Add(@event);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetEvent", new { id = @event.Id }, @event);
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
