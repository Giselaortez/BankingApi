using BankingApi.Models; // Necesitamos esto para usar la clase Transaction

namespace BankingApi.Interfaces.Repositories
{
    public interface ITransactionRepository
    {
        // Agrega una nueva transacción.
        Task AddTransactionAsync(Transaction transaction);

        // Obtiene todas las transacciones de una cuenta específica.
        Task<IEnumerable<Transaction>> GetTransactionsByAccountIdAsync(int accountId);
    }
}