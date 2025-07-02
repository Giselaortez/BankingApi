using BankingApi.Models;

namespace BankingApi.Interfaces.Repositories
{
    public interface IClientRepository
    {
        Task<Client> AddClientAsync(Client client); 
        Task<Client> GetClientByIdAsync(int id);   
    }
}