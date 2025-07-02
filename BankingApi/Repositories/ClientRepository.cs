using BankingApi.Data;
using BankingApi.Interfaces.Repositories;
using BankingApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BankingApi.Repositories
{
    public class ClientRepository : IClientRepository
    {
        private readonly BankingDbContext _context;

        public ClientRepository(BankingDbContext context)
        {
            _context = context;
        }

        public async Task<Client> AddClientAsync(Client client)
        {
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
            return client;
        }

        public async Task<Client> GetClientByIdAsync(int id)
        {
            return await _context.Clients.Include(c => c.Accounts).FirstOrDefaultAsync(c => c.Id == id);
        }
    }
}