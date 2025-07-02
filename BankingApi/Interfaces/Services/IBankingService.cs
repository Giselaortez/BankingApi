using BankingApi.Models;

namespace BankingApi.Interfaces.Services
{
    public interface IBankingService
    {
        Task<Client> CreateClientAsync(Client client);
        Task<Account> CreateAccountAsync(int clientId, decimal initialBalance);
        Task<decimal?> GetAccountBalanceAsync(string accountNumber);
        Task<Transaction> DepositAsync(string accountNumber, decimal amount);
        Task<Transaction> WithdrawAsync(string accountNumber, decimal amount);
        Task<IEnumerable<Transaction>> GetAccountTransactionsAsync(string accountNumber);
    }
}