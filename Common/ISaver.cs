using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public interface ISaver : IService
    {
        Task<bool> AddUser(User u);
        Task<bool> AddDeparature(Departure d);
        Task<bool> AddBankAccount(BankAccount b);
        Task<string> AddPurchase(string username,string id_deparature, int amount);
        Task<string> DeletePurchase(string username, string id_Purchase);

        Task<List<User>> UserGetAllData();
        Task<List<Departure>> DeparatureGetAllData();
        Task<List<BankAccount>> BankAccountGetAllData();
        Task<List<Purchase>> PurchaseGetAllData();

    }
}
