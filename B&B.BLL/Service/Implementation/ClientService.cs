using B_B.DAL.Entity;
using B_B.DAL.Repo.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B_B.BLL.Service.Implementation
{
    public class ClientService
    {
        private readonly IClientRepository _clientRepo;

        public ClientService(IClientRepository clientRepo)
        {
            _clientRepo = clientRepo;
        }

        public async Task<Client> GetClientDetailsAsync(int id)
        {
            return await _clientRepo.GetClientWithReceiptsAsync(id);
        }
        public async Task AddPaymentAsync(int receiptId, decimal amount)
        {
            await _clientRepo.AddPaymentAsync(receiptId, amount);
        }
    }

}
