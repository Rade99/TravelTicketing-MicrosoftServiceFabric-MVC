using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common
{

    public interface IPublisherSubscriber : IService
    {
        Task<bool> UserPublishActive(List<User> users);
        Task<bool> DeparaturePublishActive(List<Departure> deparatures);
        Task<bool> BankAccountPublishActive(List<BankAccount> bankAccounts);
        Task<bool> PurchasePublishActive(List<Purchase> purchases);

        Task<List<User>> GetUserActiveData();
        Task<List<Departure>> GetDeparatureActiveData();
        Task<List<BankAccount>> GetBankAccountActiveData();
        Task<List<Purchase>> GetPurchaseActiveData();
        Task<List<Purchase>> GetUserPurchases(string username);
    }
}
