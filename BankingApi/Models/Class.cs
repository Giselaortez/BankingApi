using System.Security.Principal;

namespace BankingApi.Models
{
    public class Client
        // Lo que contiene la tabla Cliente
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } 
        public decimal Income { get; set; }

        public ICollection<Account> Accounts { get; set; } // Un cliente puede tener varias cuentas
    }
}