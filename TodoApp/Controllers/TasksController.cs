using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApp.Data;
using Task = TodoApp.Models.Task;

namespace TodoApp.Controllers
{
    public class TasksController : Controller
    {
        private readonly TaskDbContext _context;

        public TasksController(TaskDbContext context)
        {
            _context = context;
        }

        // GET: Tasks
        public IActionResult Index(string sortOrder, string searchString, string startDate, string endDate)
        {
            ViewData["NameSortParam"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParam"] = sortOrder == "Date" ? "date_desc" : "Date";

            var tasks = from t in _context.Tasks select t;

            if (!string.IsNullOrEmpty(startDate) && DateTime.TryParse(startDate, out var parsedStartDate))
            {
                tasks = tasks.Where(t => t.CreatedDate >= parsedStartDate);
            }

            if (!string.IsNullOrEmpty(endDate) && DateTime.TryParse(endDate, out var parsedEndDate))
            {
                parsedEndDate = parsedEndDate.AddDays(1).AddTicks(-1);
                tasks = tasks.Where(t => t.CreatedDate <= parsedEndDate);
            }

            if (!String.IsNullOrEmpty(searchString))
            {
                tasks = tasks.Where(t => t.Name.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    tasks = tasks.OrderByDescending(t => t.Name);
                    break;
                case "Date":
                    tasks = tasks.OrderBy(t => t.CreatedDate);
                    break;
                case "date_desc":
                    tasks = tasks.OrderByDescending(t => t.CreatedDate);
                    break;
                default:
                    tasks = tasks.OrderBy(t => t.Name);
                    break;
            }

            return View(tasks.ToList());
        }


        // GET: Tasks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var task = await _context.Tasks
                .FirstOrDefaultAsync(m => m.Id == id);
            if (task == null)
            {
                return NotFound();
            }

            return View(task);
        }

        // GET: Tasks/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Tasks/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,IsCompleted")] Task task)
        {
            if (ModelState.IsValid)
            {
                task.CreatedDate = DateTime.Now;

                _context.Add(task);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return PartialView("_TaskPartialView", task);
        }

        // GET: Tasks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }
            return View(task);
        }

        // POST: Tasks/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,IsCompleted")] Task task)
        {
            if (id != task.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    task.CreatedDate = DateTime.Now;
                    _context.Update(task);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaskExists(task.Id))
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
            return View(task);
        }

        // GET: Tasks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var task = await _context.Tasks
                .FirstOrDefaultAsync(m => m.Id == id);
            if (task == null)
            {
                return NotFound();
            }

            return View(task);
        }

        // POST: Tasks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task != null)
            {
                _context.Tasks.Remove(task);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TaskExists(int id)
        {
            return _context.Tasks.Any(e => e.Id == id);
        }
    }
}
