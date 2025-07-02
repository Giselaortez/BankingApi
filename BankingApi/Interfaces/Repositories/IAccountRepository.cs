using BankingApi.Models; // con esto lusamos la clase Account

namespace BankingApi.Interfaces.Repositories
{
    public interface IAccountRepository
    {
        // Agrega una nueva cuenta.
        Task<Account> AddAccountAsync(Account account);

        // Obtiene una cuenta por su número de cuenta.
        Task<Account> GetAccountByNumberAsync(string accountNumber);

        // Obtiene una cuenta por su ID interno.
        Task<Account> GetAccountByIdAsync(int id);

        // Actualiza una cuenta existente (despues de una transaccion).
        Task UpdateAccountAsync(Account account);

        // Verifica si un número de cuenta ya existe.
        Task<bool> AccountExistsAsync(string accountNumber);
    }
}