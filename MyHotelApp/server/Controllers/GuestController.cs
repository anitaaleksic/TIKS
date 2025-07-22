namespace MyHotelApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GuestController : ControllerBase
{
    private readonly HotelContext _context;

    public GuestController(HotelContext context)
    {
        _context = context;
    }
    private bool GuestExists(string id)
    {
        return _context.Guests.Any(e => e.JMBG == id);
    }
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Guest>>> GetGuests()
    {
        return await _context.Guests.ToListAsync();
    }
    [HttpGet("{id}")]
    public async Task<ActionResult<Guest>> GetGuest(string id)
    {
        var guest = await _context.Guests.FindAsync(id);
        if (guest == null)
        {
            return NotFound();
        }
        return guest;
    }
    [HttpPost]
    public async Task<ActionResult<Guest>> PostGuest(Guest guest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        _context.Guests.Add(guest);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetGuest), new { id = guest.JMBG }, guest);
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> PutGuest(string id, Guest guest)
    {
        if (id != guest.JMBG)
        {
            return BadRequest();
        }
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        _context.Entry(guest).State = EntityState.Modified;
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!GuestExists(id))
            {
                return NotFound();
            }
            throw;
        }
        return NoContent();
    }
}