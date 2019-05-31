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
    public class OrdersController : ControllerBase
    {
        private readonly EntityRepository _context;
        private readonly IGenericRepository _handler;

        public OrdersController(EntityRepository context)
        {
            _context = context;
            _handler = new GenericEntityRepositoryHandler(_context);

        }

        // GET: api/Orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders(Order order)
        {
            try
            {
                var results = await Task.Run(async () => _handler.FindMultiple(order));

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

            //return await _context.Orders.ToListAsync();
        }

        // GET: api/Orders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            if (id == -1)
            {
                _context.Database.EnsureDeleted();
                _context.Database.EnsureCreated();

                return Accepted();
            }
            else
            {
                
            

            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            return order;

            }
        }

        // PUT: api/Orders/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrder(int id, Order order)
        {
            if (id != order.Id)
            {
                return BadRequest();
            }

            _context.Entry(order).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
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

        // POST: api/Orders
        [HttpPost]
        public async Task<ActionResult<Order>> PostOrder(Order order)
        {
            try
            {
                var result = _handler.Add(order);

                if (result)
                {
                    await _context.SaveChangesAsync();
                    return StatusCode(StatusCodes.Status201Created,
                        string.Format("Succesfully added Order, Result -> {0}", result));
                }
                else
                {
                    return StatusCode(StatusCodes.Status409Conflict, string.Format("Unable to add Order, Result -> {0}", result));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw e;
            }

            //_context.Orders.Add(order);
            //await _context.SaveChangesAsync();

            //return CreatedAtAction("GetOrder", new { id = order.Id }, order);
        }

        // DELETE: api/Orders/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Order>> DeleteOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return order;
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
    }
}
