using System.Transactions;

namespace BankingApi.Models
{
    public class Account
    {
        public int Id { get; set; }
        public string AccountNumber { get; set; } // Único
        public decimal Balance { get; set; }

        public int ClientId { get; set; }
        public Client Client { get; set; } // Relación con el cliente

        public ICollection<Transaction> Transactions { get; set; } // Una cuenta puede tener varias transacciones
    }
}