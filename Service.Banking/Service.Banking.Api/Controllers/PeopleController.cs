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
    public class PeopleController : ControllerBase
    {
        private readonly EntityRepository _context;
        private readonly IGenericRepository _handler;

        public PeopleController(EntityRepository context)
        {
            _context = context;
            _handler = new GenericEntityRepositoryHandler(_context);
        }

        // GET: api/People
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Person>>> GetPersons(Person person)
        {
            try
            {
                var results = await Task.Run(async () => _handler.FindMultiple(person));

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

            //return await _context.Persons.ToListAsync();
        }

        // GET: api/People/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Person>> GetPerson(int id)
        {
            if (id == -1)
            {
                _context.Database.EnsureDeleted();
                _context.Database.EnsureCreated();

                return Accepted();
            }
            else
            {
                var person = await _context.Persons.FindAsync(id);

                if (person == null)
                {
                    return NotFound();
                }

                return person;
            }
        }

        // PUT: api/People/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPerson(int id, Person person)
        {
            if (id != person.Id)
            {
                return BadRequest();
            }

            _context.Entry(person).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PersonExists(id))
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

        // POST: api/People
        [HttpPost]
        public async Task<ActionResult<Person>> PostPerson(Person person)
        {
            try
            {
                var result = _handler.Add(person);

                if (result)
                {
                    await _context.SaveChangesAsync();
                    return StatusCode(StatusCodes.Status201Created,
                        string.Format("Succesfully added Person, Result -> {0}", result));
                }
                else
                {
                    return StatusCode(StatusCodes.Status409Conflict, string.Format("Unable to add Person, Result -> {0}", result));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw e;
            }

            //_context.Persons.Add(person);
            //await _context.SaveChangesAsync();

            //return CreatedAtAction("GetPerson", new { id = person.Id }, person);
        }

        // DELETE: api/People/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Person>> DeletePerson(int id)
        {
            var person = await _context.Persons.FindAsync(id);
            if (person == null)
            {
                return NotFound();
            }

            _context.Persons.Remove(person);
            await _context.SaveChangesAsync();

            return person;
        }

        private bool PersonExists(int id)
        {
            return _context.Persons.Any(e => e.Id == id);
        }
    }
}
