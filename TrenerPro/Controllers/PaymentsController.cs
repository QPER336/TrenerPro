using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TrenerPro.Data;
using TrenerPro.Models;

namespace TrenerPro.Controllers
{
    [Authorize]
    public class PaymentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PaymentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Payments (Tylko płatności MOICH klientów)
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var payments = _context.Payments
                .Include(p => p.Client)
                // WARUNEK: Pokaż płatność tylko jeśli klient należy do mnie
                .Where(p => p.Client.TrainerId == userId);

            return View(await payments.ToListAsync());
        }

        // GET: Payments/Create
        public IActionResult Create()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Lista rozwijana: Pokaż tylko MOICH klientów
            ViewData["ClientId"] = new SelectList(_context.Clients.Where(c => c.TrainerId == userId), "Id", "LastName");

            return View();
        }

        // POST: Payments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ClientId,PaymentDate,Amount,Description,IsConfirmed")] Payment payment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(payment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewData["ClientId"] = new SelectList(_context.Clients.Where(c => c.TrainerId == userId), "Id", "LastName", payment.ClientId);

            return View(payment);
        }

        // Akcja potwierdzenia płatności
        public async Task<IActionResult> Confirm(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var payment = await _context.Payments
                .Include(p => p.Client)
                .FirstOrDefaultAsync(p => p.Id == id && p.Client.TrainerId == userId); 

            if (payment != null)
            {
                payment.IsConfirmed = true;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Payments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var payment = await _context.Payments
                .Include(p => p.Client)
                .FirstOrDefaultAsync(m => m.Id == id && m.Client.TrainerId == userId); 

            if (payment == null) return NotFound();

            return View(payment);
        }

        // POST: Payments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var payment = await _context.Payments
                .Include(p => p.Client)
                .FirstOrDefaultAsync(p => p.Id == id && p.Client.TrainerId == userId);

            if (payment != null)
            {
                _context.Payments.Remove(payment);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}