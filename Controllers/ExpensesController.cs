using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Data;
using ExpenseTracker.Models;

namespace ExpenseTracker.Controllers
{
    public class ExpensesController : Controller
    {
        private readonly ExpenseDbContext _context;
        private readonly List<string> _categories = new()
        {
            "Food & Dining", "Transportation", "Shopping", "Entertainment",
            "Bills & Utilities", "Healthcare", "Education", "Travel",
            "Personal Care", "Others"
        };

        public ExpensesController(ExpenseDbContext context)
        {
            _context = context;
        }

        // GET: Expenses
        public async Task<IActionResult> Index(string category, DateTime? startDate, DateTime? endDate)
        {
            ViewBag.Categories = _categories;
            ViewBag.SelectedCategory = category;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;

            var query = _context.Expenses.AsQueryable();

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(e => e.Category == category);
            }

            if (startDate.HasValue)
            {
                query = query.Where(e => e.Date >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(e => e.Date <= endDate.Value);
            }

            var expenses = await query.OrderByDescending(e => e.Date).ToListAsync();
            
            ViewBag.TotalExpenses = expenses.Sum(e => e.Amount);

            return View(expenses);
        }

        // GET: Expenses/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            // Load data into memory first, then perform aggregations
            var monthlyExpenses = await _context.Expenses
                .Where(e => e.Date.Month == currentMonth && e.Date.Year == currentYear)
                .ToListAsync();

            var categorySummary = monthlyExpenses
                .GroupBy(e => e.Category)
                .Select(g => new CategorySummary
                {
                    Category = g.Key,
                    TotalAmount = (decimal)g.Sum(e => e.Amount),
                    Count = g.Count()
                })
                .OrderByDescending(c => c.TotalAmount)
                .ToList();

            ViewBag.MonthlyTotal = monthlyExpenses.Sum(e => e.Amount);
            ViewBag.CurrentMonth = DateTime.Now.ToString("MMMM yyyy");
            ViewBag.TransactionCount = monthlyExpenses.Count;

            // Last 6 months data - load into memory first
            var sixMonthsAgo = DateTime.Now.AddMonths(-6);
            var allExpenses = await _context.Expenses
                .Where(e => e.Date >= sixMonthsAgo)
                .ToListAsync();
            
            var monthlySummary = allExpenses
                .GroupBy(e => new { e.Date.Year, e.Date.Month })
                .Select(g => new
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1),
                    Total = g.Sum(e => e.Amount)
                })
                .OrderBy(m => m.Month)
                .ToList();

            ViewBag.MonthlySummary = monthlySummary;

            return View(categorySummary);
        }

        // GET: Expenses/Create
        public IActionResult Create()
        {
            ViewBag.Categories = _categories;
            return View();
        }

        // POST: Expenses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Amount,Category,Date,Description")] Expense expense)
        {
            if (ModelState.IsValid)
            {
                _context.Add(expense);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Expense added successfully!";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Categories = _categories;
            return View(expense);
        }

        // GET: Expenses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null)
            {
                return NotFound();
            }
            ViewBag.Categories = _categories;
            return View(expense);
        }

        // POST: Expenses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Amount,Category,Date,Description,CreatedAt")] Expense expense)
        {
            if (id != expense.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(expense);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Expense updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ExpenseExists(expense.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Categories = _categories;
            return View(expense);
        }

        // GET: Expenses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var expense = await _context.Expenses
                .FirstOrDefaultAsync(m => m.Id == id);
            if (expense == null)
            {
                return NotFound();
            }

            return View(expense);
        }

        // POST: Expenses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense != null)
            {
                _context.Expenses.Remove(expense);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Expense deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ExpenseExists(int id)
        {
            return _context.Expenses.Any(e => e.Id == id);
        }
    }
}