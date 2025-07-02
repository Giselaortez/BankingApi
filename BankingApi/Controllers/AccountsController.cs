using BankingApi.Interfaces.Services;
using BankingApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace BankingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IBankingService _bankingService;

        public AccountsController(IBankingService bankingService)
        {
            _bankingService = bankingService;
        }

        [HttpPost]
        public async Task<ActionResult<Account>> CreateAccount([FromQuery] int clientId, [FromQuery] decimal initialBalance)
        {
            try
            {
                var newAccount = await _bankingService.CreateAccountAsync(clientId, initialBalance);
                return CreatedAtAction(nameof(CreateAccount), newAccount);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{accountNumber}/balance")]
        public async Task<ActionResult<decimal>> GetBalance(string accountNumber)
        {
            var balance = await _bankingService.GetAccountBalanceAsync(accountNumber);
            if (balance == null)
            {
                return NotFound("Account not found.");
            }
            return Ok(balance.Value);
        }

        [HttpPost("{accountNumber}/deposit")]
        public async Task<ActionResult<Transaction>> Deposit(string accountNumber, [FromQuery] decimal amount)
        {
            try
            {
                var transaction = await _bankingService.DepositAsync(accountNumber, amount);
                return Ok(transaction);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("{accountNumber}/withdraw")]
        public async Task<ActionResult<Transaction>> Withdraw(string accountNumber, [FromQuery] decimal amount)
        {
            try
            {
                var transaction = await _bankingService.WithdrawAsync(accountNumber, amount);
                return Ok(transaction);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message); // Fondos insuficientes
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{accountNumber}/transactions")]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactions(string accountNumber)
        {
            try
            {
                var transactions = await _bankingService.GetAccountTransactionsAsync(accountNumber);
                if (!transactions.Any())
                {
                    return NotFound("No transactions found for this account or account does not exist.");
                }
                return Ok(transactions);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
