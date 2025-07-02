using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using BankingApi.Interfaces.Repositories;
using BankingApi.Services;
using BankingApi.Models;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BankingApi.Tests
{
    [TestClass]
    public class BankingServiceTests
    {
        private Mock<IClientRepository> _mockClientRepository;
        private Mock<IAccountRepository> _mockAccountRepository;
        private Mock<ITransactionRepository> _mockTransactionRepository;
        private BankingService _bankingService;

        [TestInitialize]
        public void Setup()
        {
            _mockClientRepository = new Mock<IClientRepository>();
            _mockAccountRepository = new Mock<IAccountRepository>();
            _mockTransactionRepository = new Mock<ITransactionRepository>();
            _bankingService = new BankingService(
                _mockClientRepository.Object,
                _mockAccountRepository.Object,
                _mockTransactionRepository.Object
            );
        }

        [TestMethod]
        public async Task CreateClientAsync_ShouldReturnClient()
        {
            // Arrange
            var client = new Client { Name = "Test Client", DateOfBirth = new DateTime(1990, 1, 1), Gender = "Male", Income = 50000 };
            _mockClientRepository.Setup(repo => repo.AddClientAsync(It.IsAny<Client>()))
                                 .ReturnsAsync(client);

            // Act
            var result = await _bankingService.CreateClientAsync(client);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Test Client", result.Name);
            _mockClientRepository.Verify(repo => repo.AddClientAsync(It.IsAny<Client>()), Times.Once);
        }

        [TestMethod]
        public async Task CreateAccountAsync_ValidClientId_ShouldReturnAccount()
        {
            // Arrange
            var client = new Client { Id = 1, Name = "Test Client" };
            _mockClientRepository.Setup(repo => repo.GetClientByIdAsync(1))
                                 .ReturnsAsync(client);
            _mockAccountRepository.Setup(repo => repo.AccountExistsAsync(It.IsAny<string>()))
                                 .ReturnsAsync(false); // Simula que el número de cuenta es único
            _mockAccountRepository.Setup(repo => repo.AddAccountAsync(It.IsAny<Account>()))
                                 .ReturnsAsync((Account acc) => acc);

            // Act
            var result = await _bankingService.CreateAccountAsync(1, 100m);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(100m, result.Balance);
            Assert.AreEqual(1, result.ClientId);
            _mockAccountRepository.Verify(repo => repo.AddAccountAsync(It.IsAny<Account>()), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CreateAccountAsync_InvalidClientId_ShouldThrowException()
        {
            // Arrange
            _mockClientRepository.Setup(repo => repo.GetClientByIdAsync(It.IsAny<int>()))
                                 .ReturnsAsync((Client)null);

            // Act
            await _bankingService.CreateAccountAsync(999, 100m);

            // Assert (ExpectedException se encarga de esto)
        }

        [TestMethod]
        public async Task DepositAsync_ValidDeposit_ShouldIncreaseBalanceAndAddTransaction()
        {
            // Arrange
            var account = new Account { Id = 1, AccountNumber = "12345", Balance = 100m };
            _mockAccountRepository.Setup(repo => repo.GetAccountByNumberAsync("12345"))
                                 .ReturnsAsync(account);
            _mockAccountRepository.Setup(repo => repo.UpdateAccountAsync(It.IsAny<Account>()))
                                 .Returns(Task.CompletedTask);
            _mockTransactionRepository.Setup(repo => repo.AddTransactionAsync(It.IsAny<Transaction>()))
                                    .Returns(Task.CompletedTask);

            // Act
            var transaction = await _bankingService.DepositAsync("12345", 50m);

            // Assert
            Assert.IsNotNull(transaction);
            Assert.AreEqual(150m, account.Balance);
            Assert.AreEqual(TransactionType.Deposit, transaction.Type);
            Assert.AreEqual(50m, transaction.Amount);
            Assert.AreEqual(150m, transaction.BalanceAfterTransaction);
            _mockAccountRepository.Verify(repo => repo.UpdateAccountAsync(account), Times.Once);
            _mockTransactionRepository.Verify(repo => repo.AddTransactionAsync(It.IsAny<Transaction>()), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task DepositAsync_NegativeAmount_ShouldThrowException()
        {
            // Act
            await _bankingService.DepositAsync("12345", -10m);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task DepositAsync_AccountNotFound_ShouldThrowException()
        {
            // Arrange
            _mockAccountRepository.Setup(repo => repo.GetAccountByNumberAsync(It.IsAny<string>()))
                                 .ReturnsAsync((Account)null);

            // Act
            await _bankingService.DepositAsync("nonexistent", 50m);
        }


        [TestMethod]
        public async Task WithdrawAsync_SufficientFunds_ShouldDecreaseBalanceAndAddTransaction()
        {
            // Arrange
            var account = new Account { Id = 1, AccountNumber = "12345", Balance = 100m };
            _mockAccountRepository.Setup(repo => repo.GetAccountByNumberAsync("12345"))
                                 .ReturnsAsync(account);
            _mockAccountRepository.Setup(repo => repo.UpdateAccountAsync(It.IsAny<Account>()))
                                 .Returns(Task.CompletedTask);
            _mockTransactionRepository.Setup(repo => repo.AddTransactionAsync(It.IsAny<Transaction>()))
                                    .Returns(Task.CompletedTask);

            // Act
            var transaction = await _bankingService.WithdrawAsync("12345", 50m);

            // Assert
            Assert.IsNotNull(transaction);
            Assert.AreEqual(50m, account.Balance);
            Assert.AreEqual(TransactionType.Withdrawal, transaction.Type);
            Assert.AreEqual(50m, transaction.Amount);
            Assert.AreEqual(50m, transaction.BalanceAfterTransaction);
            _mockAccountRepository.Verify(repo => repo.UpdateAccountAsync(account), Times.Once);
            _mockTransactionRepository.Verify(repo => repo.AddTransactionAsync(It.IsAny<Transaction>()), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task WithdrawAsync_InsufficientFunds_ShouldThrowException()
        {
            // Arrange
            var account = new Account { Id = 1, AccountNumber = "12345", Balance = 10m };
            _mockAccountRepository.Setup(repo => repo.GetAccountByNumberAsync("12345"))
                                 .ReturnsAsync(account);

            // Act
            await _bankingService.WithdrawAsync("12345", 50m);

            // Assert (ExpectedException se encarga de esto)
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task WithdrawAsync_NegativeAmount_ShouldThrowException()
        {
            // Act
            await _bankingService.WithdrawAsync("12345", -10m);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task WithdrawAsync_AccountNotFound_ShouldThrowException()
        {
            // Arrange
            _mockAccountRepository.Setup(repo => repo.GetAccountByNumberAsync(It.IsAny<string>()))
                                 .ReturnsAsync((Account)null);

            // Act
            await _bankingService.WithdrawAsync("nonexistent", 50m);
        }


        [TestMethod]
        public async Task GetAccountBalanceAsync_AccountExists_ShouldReturnBalance()
        {
            // Arrange
            var account = new Account { AccountNumber = "12345", Balance = 200m };
            _mockAccountRepository.Setup(repo => repo.GetAccountByNumberAsync("12345"))
                                 .ReturnsAsync(account);

            // Act
            var balance = await _bankingService.GetAccountBalanceAsync("12345");

            // Assert
            Assert.AreEqual(200m, balance);
        }

        [TestMethod]
        public async Task GetAccountBalanceAsync_AccountNotFound_ShouldReturnNull()
        {
            // Arrange
            _mockAccountRepository.Setup(repo => repo.GetAccountByNumberAsync(It.IsAny<string>()))
                                 .ReturnsAsync((Account)null);

            // Act
            var balance = await _bankingService.GetAccountBalanceAsync("nonexistent");

            // Assert
            Assert.IsNull(balance);
        }

        [TestMethod]
        public async Task GetAccountTransactionsAsync_AccountExists_ShouldReturnTransactions()
        {
            // Arrange
            var account = new Account { Id = 1, AccountNumber = "12345" };
            var transactions = new List<Transaction>
            {
                new Transaction { Id = 1, AccountId = 1, Type = TransactionType.Deposit, Amount = 100, TransactionDate = DateTime.UtcNow, BalanceAfterTransaction = 100 },
                new Transaction { Id = 2, AccountId = 1, Type = TransactionType.Withdrawal, Amount = 50, TransactionDate = DateTime.UtcNow, BalanceAfterTransaction = 50 }
            };

            _mockAccountRepository.Setup(repo => repo.GetAccountByNumberAsync("12345"))
                                 .ReturnsAsync(account);
            _mockTransactionRepository.Setup(repo => repo.GetTransactionsByAccountIdAsync(1))
                                     .ReturnsAsync(transactions);

            // Act
            var result = await _bankingService.GetAccountTransactionsAsync("12345");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
            _mockTransactionRepository.Verify(repo => repo.GetTransactionsByAccountIdAsync(1), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GetAccountTransactionsAsync_AccountNotFound_ShouldThrowException()
        {
            // Arrange
            _mockAccountRepository.Setup(repo => repo.GetAccountByNumberAsync(It.IsAny<string>()))
                                 .ReturnsAsync((Account)null);

            // Act
            await _bankingService.GetAccountTransactionsAsync("nonexistent");
        }
    }
}