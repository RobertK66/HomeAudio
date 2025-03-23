using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebRadioImpl;

namespace DbWebApi
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebRadiosController2 : ControllerBase
    {
        private readonly MyDataContext _context;

        public WebRadiosController2(MyDataContext context)
        {
            _context = context;
        }

        // GET: api/WebRadiosController2
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WebRadio>>> GetWebRadios()
        {
            return await _context.WebRadios.ToListAsync();
        }

        // GET: api/WebRadiosController2/5
        [HttpGet("{id}")]
        public async Task<ActionResult<WebRadio>> GetWebRadio(int id)
        {
            var webRadio = await _context.WebRadios.FindAsync(id);

            if (webRadio == null)
            {
                return NotFound();
            }

            return webRadio;
        }

        // PUT: api/WebRadiosController2/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutWebRadio(int id, WebRadio webRadio)
        {
            if (id != webRadio.Id)
            {
                return BadRequest();
            }

            _context.Entry(webRadio).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!WebRadioExists(id))
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

        // POST: api/WebRadiosController2
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<WebRadio>> PostWebRadio(WebRadio webRadio)
        {
            _context.WebRadios.Add(webRadio);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetWebRadio", new { id = webRadio.Id }, webRadio);
        }

        // DELETE: api/WebRadiosController2/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWebRadio(int id)
        {
            var webRadio = await _context.WebRadios.FindAsync(id);
            if (webRadio == null)
            {
                return NotFound();
            }

            _context.WebRadios.Remove(webRadio);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool WebRadioExists(int id)
        {
            return _context.WebRadios.Any(e => e.Id == id);
        }
    }
}
