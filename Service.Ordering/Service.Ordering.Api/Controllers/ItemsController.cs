using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service.Ordering.Domain.Concrete;
using Service.Ordering.Repository.EntityFramework;
using Shared.Repository.Core;

namespace Service.Ordering.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private readonly EntityRepository _context;
        private readonly IGenericRepository _handler;

        public ItemsController(EntityRepository context)
        {
            _context = context;
            _handler = new GenericEntityRepositoryHandler(_context);
        }

        // GET: api/Items
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Item>>> GetItems(Item item)
        {
            try
            {
                var results = await Task.Run(async () => _handler.FindMultiple(item));

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
            
            //return await _context.Items.ToListAsync();
        }

        // GET: api/Items/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Item>> GetItem(int id)
        {
            if (id == -1)
            {
                _context.Database.EnsureDeleted();
                _context.Database.EnsureCreated();

                return Accepted();
            }
            else
            {

                var item = await _context.Items.FindAsync(id);

                if (item == null)
                {
                    return NotFound();
                }

                return item;
            }
        }

        // PUT: api/Items/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutItem(int id, Item item)
        {
            if (id != item.Id)
            {
                return BadRequest();
            }

            _context.Entry(item).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ItemExists(id))
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

        // POST: api/Items
        [HttpPost]
        public async Task<ActionResult<Item>> PostItem(Item item)
        {
            try
            {
                var result = _handler.Add(item);

                if (result)
                {
                    await _context.SaveChangesAsync();
                    return StatusCode(StatusCodes.Status201Created,
                        string.Format("Succesfully added item, Result -> {0}", result));
                }
                else
                {
                    return StatusCode(StatusCodes.Status409Conflict, string.Format("Unable to add Item, Result -> {0}", result));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw e;
            }

            //_context.Items.Add(item);
            //await _context.SaveChangesAsync();

            //return CreatedAtAction("GetItem", new { id = item.Id }, item);
        }

        // DELETE: api/Items/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Item>> DeleteItem(int id)
        {
            var item = await _context.Items.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            _context.Items.Remove(item);
            await _context.SaveChangesAsync();

            return item;
        }

        private bool ItemExists(int id)
        {
            return _context.Items.Any(e => e.Id == id);
        }
    }
}
