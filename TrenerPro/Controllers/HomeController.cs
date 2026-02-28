using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; 
using System.Diagnostics;
using TrenerPro.Data; 
using TrenerPro.Models;

namespace TrenerPro.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context; 

        // Konstruktor wstrzykujący bazę danych
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Dashboard");
            }
            return View();
        }

        // --- DASHBOARD Z FILTROWANIEM ---
        public async Task<IActionResult> Dashboard()
        {
            if (!User.Identity.IsAuthenticated) return RedirectToAction("Index");

            // Pobieramy ID zalogowanego trenera
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var viewModel = new DashboardViewModel();

            // 1. Liczymy TYLKO moich klientów
            viewModel.ActiveClientsCount = await _context.Clients
                .Where(c => c.TrainerId == userId)
                .CountAsync();

            // 2. Przychód w bieżącym miesiącu (Z moich klientów)
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            // Tutaj musimy sięgnąć do tabeli Payments, ale sprawdzić, 
            // czy Klient przypisany do płatności należy do mnie.
            viewModel.MonthlyRevenue = await _context.Payments
                .Include(p => p.Client) //  TrainerId
                .Where(p => p.IsConfirmed
                         && p.PaymentDate.Month == currentMonth
                         && p.PaymentDate.Year == currentYear
                         && p.Client.TrainerId == userId) 
                .SumAsync(p => p.Amount);

            // 3. Kończące się plany (tylko u moich klientów)
            viewModel.ExpiringClients = await _context.Clients
                .Where(c => c.TrainerId == userId
                         && c.PlanEndDate >= DateTime.Now
                         && c.PlanEndDate <= DateTime.Now.AddDays(7))
                .ToListAsync();

            // 4. Nieopłacone (tylko u moich klientów)
            viewModel.PendingPayments = await _context.Payments
                .Include(p => p.Client)
                .Where(p => !p.IsConfirmed && p.Client.TrainerId == userId)
                .OrderBy(p => p.PaymentDate)
                .Take(5)
                .ToListAsync();

            return View(viewModel);
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}