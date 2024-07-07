using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;

[Route("api/[controller]")]
[ApiController]
public class BudgetsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public BudgetsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/budgets
    [HttpGet("{userId}")]
    public async Task<ActionResult<IEnumerable<Budget>>> GetBudgets(int userId)
    {
        var budgets = await _context.Budgets.Where(b => b.user_id == userId).ToListAsync();

        if (budgets == null || !budgets.Any())
        {
            return NotFound(new { message = "No budgets found for the specified user." });
        }

        return Ok(budgets);
    }

    // GET: api/budgets/5
    [HttpGet("{userId}/{id}")]
    public async Task<ActionResult<Budget>> GetBudget(int userId, int id)
    {
        var budget = await _context.Budgets
                                    .Where(b => b.user_id == userId && b.id == id)
                                    .FirstOrDefaultAsync();

        if (budget == null)
        {
            return NotFound(new { message = "Budget not found for the specified user and ID." });
        }

        return budget;
    }

    // Modify Budget: api/budgets/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpadateBudget(int id, Budget budget)
    {
        if (id != budget.id)
        {
            return BadRequest();
        }
        _context.Entry(budget).State = EntityState.Modified;

        // Ensure DateTime properties are UTC
        if (budget.start_date.HasValue)
            budget.start_date = budget.start_date.Value.ToUniversalTime();

        if (budget.end_date.HasValue)
            budget.end_date = budget.end_date.Value.ToUniversalTime();

        _context.Entry(budget).State = EntityState.Modified;
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!BudgetExists(id))
            {
                return NotFound(new { message = "Budget Not Found!" });
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // Create Budget: api/budgets
    [HttpPost]
    public async Task<ActionResult<Budget>> CreateBudget(Budget budget)
    {
        var user = await _context.Users.FindAsync(budget.user_id);
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        // Ensure DateTime properties are UTC
        if (budget.start_date.HasValue)
            budget.start_date = budget.start_date.Value.ToUniversalTime();

        if (budget.end_date.HasValue)
            budget.end_date = budget.end_date.Value.ToUniversalTime();

        budget.remaining_amount = budget.total_amount;

        _context.Budgets.Add(budget);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetBudget), new { id = budget.id }, budget);
    }

    // DELETE Budget: api/budgets/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBudget(int id)
    {
        var budget = await _context.Budgets.FindAsync(id);
        if (budget == null)
        {
            return NotFound(new { message = "Budget Not Found!" });
        }
        // Find all expenses associated with the budget
        var expenses = _context.Expenses.Where(e => e.budget_id == id);
        _context.Expenses.RemoveRange(expenses);

        //Remove budget
        _context.Budgets.Remove(budget);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool BudgetExists(int id)
    {
        return _context.Budgets.Any(e => e.id == id);
    }
}