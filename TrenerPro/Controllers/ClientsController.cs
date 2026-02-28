using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TrenerPro.Data;
using TrenerPro.Models;

namespace TrenerPro.Controllers
{
    [Authorize]
    public class ClientsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClientsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Clients
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Pokaż tylko moich klientów
            return View(await _context.Clients
                .Where(c => c.TrainerId == userId)
                .ToListAsync());
        }

        // GET: Clients/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var client = await _context.Clients
                .Include(m => m.Payments)
                // ZABEZPIECZENIE: Sprawdzamy czy ID klienta pasuje ORAZ czy należy do trenera
                .FirstOrDefaultAsync(m => m.Id == id && m.TrainerId == userId);

            if (client == null) return NotFound();

            return View(client);
        }

        // GET: Clients/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Clients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,Age,StartWeight,CurrentWeight,ServiceType,JoinDate,PlanEndDate")] Client client)
        {
            // 1. Pobierz ID zalogowanego trenera
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 2. Przypisz klienta do tego trenera
            client.TrainerId = userId;

            if (ModelState.IsValid)
            {
                // Zapisujemy Klienta
                _context.Add(client);
                await _context.SaveChangesAsync();

                // Tworzymy płatność startową
                var initialPayment = new Payment
                {
                    ClientId = client.Id,
                    PaymentDate = DateTime.Now,
                    Amount = 0,
                    Description = "Płatność startowa / Pierwszy miesiąc",
                    IsConfirmed = false
                };

                _context.Add(initialPayment);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Payments"); // Lub przekieruj do Index Klientów
            }
            return View(client);
        }

        // GET: Clients/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // ZABEZPIECZENIE: Pobieramy tylko, jeśli należy do trenera
            var client = await _context.Clients
                .FirstOrDefaultAsync(c => c.Id == id && c.TrainerId == userId);

            if (client == null) return NotFound();

            return View(client);
        }

        // POST: Clients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,Age,StartWeight,CurrentWeight,ServiceType,JoinDate,PlanEndDate")] Client client)
        {
            if (id != client.Id) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // ZABEZPIECZENIE: Upewniamy się, że przy zapisie TrainerId się nie zgubił
            // i że wciąż jest przypisany do zalogowanego użytkownika
            client.TrainerId = userId;

            if (ModelState.IsValid)
            {
                try
                {
                    // Sprawdzamy czy taki rekord w ogóle istnieje w bazie dla tego usera
                    if (!await _context.Clients.AnyAsync(c => c.Id == id && c.TrainerId == userId))
                    {
                        return NotFound();
                    }

                    _context.Update(client);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClientExists(client.Id))
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
            return View(client);
        }

        // POST: Clients/UpdateNotes
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateNotes(int id, string notes)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // ZABEZPIECZENIE: Szukamy klienta z uwzględnieniem trenera
            var client = await _context.Clients
                .FirstOrDefaultAsync(c => c.Id == id && c.TrainerId == userId);

            if (client == null) return NotFound();

            client.Notes = notes;

            _context.Update(client);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = id });
        }

        // GET: Clients/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var client = await _context.Clients
                .FirstOrDefaultAsync(m => m.Id == id && m.TrainerId == userId);

            if (client == null) return NotFound();

            return View(client);
        }

        // POST: Clients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // ZABEZPIECZENIE: Usuwamy tylko jeśli należy do trenera
            var client = await _context.Clients
                .FirstOrDefaultAsync(c => c.Id == id && c.TrainerId == userId);

            if (client != null)
            {
                _context.Clients.Remove(client);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ClientExists(int id)
        {
            // Tutaj też warto sprawdzać trenera, choć w Edit już to robimy
            return _context.Clients.Any(e => e.Id == id);
        }

        // POST: Clients/AddNextPayment/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNextPayment(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Pobieramy klienta (ZABEZPIECZENIE: sprawdzamy TrainerId)
            var client = await _context.Clients
                .Include(c => c.Payments)
                .FirstOrDefaultAsync(c => c.Id == id && c.TrainerId == userId);

            if (client == null)
            {
                return NotFound(); // Klient nie istnieje lub nie należy do Ciebie
            }

            // Ustalamy parametry nowej płatności
            DateTime newDate = DateTime.Now;
            decimal newAmount = 150;
            string newDescription = "";

            var lastPayment = client.Payments.OrderByDescending(p => p.PaymentDate).FirstOrDefault();

            if (lastPayment != null)
            {
                newDate = lastPayment.PaymentDate.AddMonths(1);
                newAmount = lastPayment.Amount;

                System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo("pl-PL");
                newDescription = newDate.ToString("MMMM yyyy", culture);
                newDescription = char.ToUpper(newDescription[0]) + newDescription.Substring(1);
            }
            else
            {
                newDescription = "Płatność startowa";
            }

            var nextPayment = new Payment
            {
                ClientId = id,
                PaymentDate = newDate,
                Amount = newAmount,
                Description = newDescription,
                IsConfirmed = false
            };

            _context.Payments.Add(nextPayment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = id });
        }
    }
}