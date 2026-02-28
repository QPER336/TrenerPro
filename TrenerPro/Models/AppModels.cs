using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrenerPro.Models
{
    // Typ usługi (Dropdown)
    public enum ServiceType
    {
        // To wyświetli się na liście:
        [Display(Name = "Trening Personalny")]
        PersonalTraining,

        // To wyświetli się na liście:
        [Display(Name = "Prowadzenie Online")]
        OnlineCoaching
    }

    public class Client
    {
        public int Id { get; set; }
        public string? TrainerId { get; set; }

        [Required(ErrorMessage = "Imię jest wymagane")]
        [Display(Name = "Imię")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nazwisko jest wymagane")]
        [Display(Name = "Nazwisko")]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Wiek")]
        public int Age { get; set; }

        [Display(Name = "Waga Startowa (kg)")]
        [Column(TypeName = "decimal(5, 2)")]
        public decimal StartWeight { get; set; }

        [Display(Name = "Waga Aktualna (kg)")]
        [Column(TypeName = "decimal(5, 2)")]
        public decimal CurrentWeight { get; set; }

        [Display(Name = "Rodzaj Usługi")]
        public ServiceType ServiceType { get; set; }

        [Display(Name = "Data dołączenia")]
        [DataType(DataType.Date)]
        public DateTime JoinDate { get; set; } = DateTime.Now;
        [Display(Name = "Koniec Planu / Bloku")]
        [DataType(DataType.Date)]
        public DateTime PlanEndDate { get; set; } = DateTime.Now.AddMonths(1); // Domyślnie 1 miesiąc
        // Relacja: Jeden klient ma wiele płatności
        public List<Payment> Payments { get; set; } = new List<Payment>();
        // Helper do wyświetlania pełnej nazwy
        public string FullName => $"{FirstName} {LastName}";

        [Display(Name = "Notatki Trenera (Poufne)")]
        public string? Notes { get; set; }
    }
    public class DashboardViewModel
    {
        public int ActiveClientsCount { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public List<Client> ExpiringClients { get; set; } = new List<Client>();
        public List<Payment> PendingPayments { get; set; } = new List<Payment>();
    }
    public class Payment
    {
        public int Id { get; set; }

        [Display(Name = "Kwota (PLN)")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        [Display(Name = "Data płatności")]
        [DataType(DataType.Date)]
        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [Display(Name = "Opis (np. Styczeń 2024)")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Zatwierdzona?")]
        public bool IsConfirmed { get; set; } = false; // Domyślnie niezatwierdzona

        // Relacja z klientem
        public int ClientId { get; set; }
        public Client? Client { get; set; }
    }
}