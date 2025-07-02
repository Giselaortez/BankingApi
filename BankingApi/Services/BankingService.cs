using BankingApi.Interfaces.Repositories;
using BankingApi.Interfaces.Services;
using BankingApi.Models;

namespace BankingApi.Services
{
    public class BankingService : IBankingService
    {
        private readonly IClientRepository _clientRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly ITransactionRepository _transactionRepository;

        public BankingService(IClientRepository clientRepository,
                              IAccountRepository accountRepository,
                              ITransactionRepository transactionRepository)
        {
            _clientRepository = clientRepository;
            _accountRepository = accountRepository;
            _transactionRepository = transactionRepository;
        }

        public async Task<Client> CreateClientAsync(Client client)
        {
            return await _clientRepository.AddClientAsync(client);
        }

        public async Task<Account> CreateAccountAsync(int clientId, decimal initialBalance)
        {
            var client = await _clientRepository.GetClientByIdAsync(clientId);
            if (client == null)
            {
                throw new ArgumentException("Client not found.");
            }

            string newAccountNumber;
            do
            {
                newAccountNumber = GenerateAccountNumber();
            } while (await _accountRepository.AccountExistsAsync(newAccountNumber)); // Asegurar que el número de cuenta sea único

            var account = new Account
            {
                ClientId = clientId,
                AccountNumber = newAccountNumber,
                Balance = initialBalance
            };

            return await _accountRepository.AddAccountAsync(account);
        }

        public async Task<decimal?> GetAccountBalanceAsync(string accountNumber)
        {
            var account = await _accountRepository.GetAccountByNumberAsync(accountNumber);
            return account?.Balance;
        }

        public async Task<Transaction> DepositAsync(string accountNumber, decimal amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("Deposit amount must be positive.");
            }

            var account = await _accountRepository.GetAccountByNumberAsync(accountNumber);
            if (account == null)
            {
                throw new ArgumentException("Account not found.");
            }

            account.Balance += amount;
            await _accountRepository.UpdateAccountAsync(account);

            var transaction = new Transaction
            {
                AccountId = account.Id,
                Type = TransactionType.Deposit,
                Amount = amount,
                TransactionDate = DateTime.UtcNow,
                BalanceAfterTransaction = account.Balance
            };
            await _transactionRepository.AddTransactionAsync(transaction);

            return transaction;
        }

        public async Task<Transaction> WithdrawAsync(string accountNumber, decimal amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("Withdrawal amount must be positive.");
            }

            var account = await _accountRepository.GetAccountByNumberAsync(accountNumber);
            if (account == null)
            {
                throw new ArgumentException("Account not found.");
            }

            if (account.Balance < amount)
            {
                throw new InvalidOperationException("Insufficient funds.");
            }

            account.Balance -= amount;
            await _accountRepository.UpdateAccountAsync(account);

            var transaction = new Transaction
            {
                AccountId = account.Id,
                Type = TransactionType.Withdrawal,
                Amount = amount,
                TransactionDate = DateTime.UtcNow,
                BalanceAfterTransaction = account.Balance
            };
            await _transactionRepository.AddTransactionAsync(transaction);

            return transaction;
        }

        public async Task<IEnumerable<Transaction>> GetAccountTransactionsAsync(string accountNumber)
        {
            var account = await _accountRepository.GetAccountByNumberAsync(accountNumber);
            if (account == null)
            {
                throw new ArgumentException("Account not found.");
            }
            return await _transactionRepository.GetTransactionsByAccountIdAsync(account.Id);
        }

        // este metodo ayuda para generar números de cuenta 
        private string GenerateAccountNumber()
        {
            Random rand = new Random();
            const string chars = "0123456789";
            return new string(Enumerable.Repeat(chars, 10)
              .Select(s => s[rand.Next(s.Length)]).ToArray());
        }
    }
}