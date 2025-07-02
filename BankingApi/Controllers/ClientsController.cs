using BankingApi.Interfaces.Services;
using BankingApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace BankingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly IBankingService _bankingService;

        public ClientsController(IBankingService bankingService)
        {
            _bankingService = bankingService;
        }

        [HttpPost]
        public async Task<ActionResult<Client>> CreateClient([FromBody] Client client)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var newClient = await _bankingService.CreateClientAsync(client);
            return CreatedAtAction(nameof(CreateClient), newClient); // Devuelve 201 Created
        }
    }
}