using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;

[Route("api/[controller]")]
[ApiController]
public class ExpensesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ExpensesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/expenses
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Expense>>> GetExpenses()
    {
        return await _context.Expenses.ToListAsync();
    }

    // GET: api/expenses/{budgetId}
    [HttpGet("{budgetId}")]
    public async Task<ActionResult<IEnumerable<Expense>>> GetExpenses(int budgetId)
    {
        var expenses = await _context.Expenses
                                     .Where(e => e.budget_id == budgetId)
                                     .ToListAsync();

        if (expenses == null || !expenses.Any())
        {
            return NotFound(new { message = "No expenses found for the specified budget." });
        }

        return Ok(expenses);
    }

    // GET: api/expenses/{budgetId}/{id}
    [HttpGet("{budgetId}/{id}")]
    public async Task<ActionResult<Expense>> GetExpense(int budgetId, int id)
    {
        var expense = await _context.Expenses
                                    .Where(e => e.budget_id == budgetId && e.id == id)
                                    .FirstOrDefaultAsync();

        if (expense == null)
        {
            return NotFound(new { message = "Expense not found for the specified budget and ID." });
        }

        return Ok(expense);
    }

    // PUT: api/expenses/5
    [HttpPut("{id}")]
    public async Task<IActionResult> ModifyExpense(int id, Expense expense)
    {
        if (id != expense.id)
        {
            return BadRequest();
        }
        if (expense.date.HasValue)
            expense.date = expense.date.Value.ToUniversalTime();


        _context.Entry(expense).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ExpenseExsits(id))
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

    // POST: api/expenses
    [HttpPost]
    public async Task<ActionResult<Expense>> CreateExpense(Expense expense)
    {
        if (expense.date.HasValue)
            expense.date = expense.date.Value.ToUniversalTime();

        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync();
        // Find the associated budget
        var budget = await _context.Budgets.FindAsync(expense.budget_id);
        if (budget == null)
        {
            return NotFound(new { message = "Budget not found" });
        }

        // Update the remaining amount in the budget
        budget.remaining_amount -= expense.amount;

        // Save the changes to the budget
        _context.Budgets.Update(budget);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetExpense), new { id = expense.id }, expense);
    }

    // DELETE: api/expenses/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteExpense(int id)
    {
        var expense = await _context.Expenses.FindAsync(id);
        if (expense == null)
        {
            return NotFound(new { message = "Expense Not Found!" });
        }

        var budget = await _context.Budgets.FindAsync(expense.budget_id); // Assuming Expense has a BudgetId property
        if (budget == null)
        {
            return NotFound(new { message = "Associated Budget Not Found!" });
        }

        // Add the expense amount back to the budget's remaining amount
        budget.remaining_amount += expense.amount;

        // Update the budget in the context
        _context.Budgets.Update(budget);

        _context.Expenses.Remove(expense);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ExpenseExsits(int id)
    {
        return _context.Expenses.Any(e => e.id == id);
    }
}