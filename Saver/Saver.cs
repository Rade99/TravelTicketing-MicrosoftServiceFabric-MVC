using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Saver
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class Saver : StatefulService, ISaver
    {
        IReliableDictionary<string, User> userDict;
        IReliableDictionary<string, Departure> deparatureDict;
        IReliableDictionary<string, BankAccount> bankAccountDict;
        IReliableDictionary<string, Purchase> purchaseDict;

        public Saver(StatefulServiceContext context)
            : base(context)
        { }

        #region GetByKey
        public async Task<BankAccount> GetBankAccountByNumber(string number)
        {
            var stateManager = this.StateManager;

            bankAccountDict = await stateManager.GetOrAddAsync<IReliableDictionary<string, BankAccount>>("BankAccountActiveData");
            using (var tx = stateManager.CreateTransaction())
            {
                var bankAccount = await bankAccountDict.TryGetValueAsync(tx, number);
                return bankAccount.Value;
            }
        }

        public async Task<Departure> GetDepartureById(string departure_id)
        {
            var stateManager = this.StateManager;

            deparatureDict = await stateManager.GetOrAddAsync<IReliableDictionary<string, Departure>>("DeparatureActiveData");
            using (var tx = stateManager.CreateTransaction())
            {
                var departure = await deparatureDict.TryGetValueAsync(tx, departure_id);
                return departure.Value;
            }
        }

        public async Task<User> GetUserByUsername(string username)
        {
            var stateManager = this.StateManager;

            userDict = await stateManager.GetOrAddAsync<IReliableDictionary<string, User>>("UserActiveData");
            using (var tx = stateManager.CreateTransaction())
            {
                var user = await userDict.TryGetValueAsync(tx, username);
                return user.Value;
            }
        }
        #endregion

        public string PurchaseCheck(string accountNumber, string idDeparture, int amount)
        {
            BankAccount bankAccount = GetBankAccountByNumber(accountNumber).Result;
            Departure departure = GetDepartureById(idDeparture).Result;

            if (bankAccount.Money < departure.Price * amount)
            {
                return "Nemate Dovoljno Novca Na Vasem Racunu";
            }
            else if(departure.NumberOfAvalableTickets < amount)
            {
                return "Nema Dovoljno Slobodnih Mesta";
            }

            return "";
        }

        #region Updates
        

        public async Task UpdateUser(User user, string purchase_id, string type)
        {
            var stateManager = this.StateManager;
            if (type == "AddPurchase")
            {
                user.PurchaseList.Add(purchase_id);
            }
            else
            {
                user.PurchaseList.RemoveAll(x => x == purchase_id);
            }
           

            userDict = await stateManager.GetOrAddAsync<IReliableDictionary<string, User>>("UserActiveData");
            using (var tx = stateManager.CreateTransaction())
            {
                await userDict.AddOrUpdateAsync(tx, user.Username, user, (k, v) => v);
                await tx.CommitAsync();
            }

            List<User> users = await UserGetAllData();
            FabricClient fabricClient = new System.Fabric.FabricClient();
            int partitionsNumber = (await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/SistemZaKupovinuKarata/PublisherSubscriber"))).Count;
            int index = 0;

            for (int i = 0; i < partitionsNumber; i++)
            {
                var proxy = ServiceProxy.Create<IPublisherSubscriber>(
                new Uri("fabric:/SistemZaKupovinuKarata/PublisherSubscriber"),
                new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(index % partitionsNumber)
                );

                bool tempPublish = await proxy.UserPublishActive(users);


                index++;
            }


        }

        public async Task UpdateBankAccount(string accountNumber, string idDeparture ,int amount, string type)
        {
            var stateManager = this.StateManager;

            BankAccount bankAccount = GetBankAccountByNumber(accountNumber).Result;
            Departure departure = GetDepartureById(idDeparture).Result;

            if(type == "AddMoney")
            {
                bankAccount.Money += departure.Price * amount;
            }
            else
            {
                bankAccount.Money -= departure.Price * amount;
            }

            bankAccountDict = await stateManager.GetOrAddAsync<IReliableDictionary<string, BankAccount>>("BankAccountActiveData");
            using (var tx = stateManager.CreateTransaction())
            {
                await bankAccountDict.AddOrUpdateAsync(tx, bankAccount.Number, bankAccount, (k,v) => v);
                await tx.CommitAsync();
            }

            List<BankAccount> bankAccounts = await BankAccountGetAllData();
            FabricClient fabricClient = new System.Fabric.FabricClient();
            int partitionsNumber = (await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/SistemZaKupovinuKarata/PublisherSubscriber"))).Count;
            int index = 0;

            for (int i = 0; i < partitionsNumber; i++)
            {
                var proxy = ServiceProxy.Create<IPublisherSubscriber>(
                new Uri("fabric:/SistemZaKupovinuKarata/PublisherSubscriber"),
                new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(index % partitionsNumber)
                );

                await proxy.BankAccountPublishActive(bankAccounts);

                index++;
            }

        }

        public async Task UpdateDeparature(string id_deparature, int amount, string type)
        {
            var stateManager = this.StateManager;

            Departure departure = GetDepartureById(id_deparature).Result;

            if(type == "AddTickets")
            {
                departure.NumberOfAvalableTickets += amount;
            }
            else
            {
                departure.NumberOfAvalableTickets -= amount;
            }
          

            deparatureDict = await stateManager.GetOrAddAsync<IReliableDictionary<string, Departure>>("DeparatureActiveData");
            using (var tx = stateManager.CreateTransaction())
            {
                await deparatureDict.AddOrUpdateAsync(tx, departure.Id, departure, (k,v) => v);
                await tx.CommitAsync();
            }

            List<Departure> deparatures = await DeparatureGetAllData();
            FabricClient fabricClient = new System.Fabric.FabricClient();
            int partitionsNumber = (await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/SistemZaKupovinuKarata/PublisherSubscriber"))).Count;
            int index = 0;

            for (int i = 0; i < partitionsNumber; i++)
            {
                var proxy = ServiceProxy.Create<IPublisherSubscriber>(
                new Uri("fabric:/SistemZaKupovinuKarata/PublisherSubscriber"),
                new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(index % partitionsNumber)
                );

                await proxy.DeparaturePublishActive(deparatures);

                index++;
            }

        }
        #endregion

        #region Add
        public async Task<string> AddPurchase(string username, string id_deparature, int amount)
        {
            var stateManager = this.StateManager;
            bool result = true;

            Purchase p = new Purchase(IdGenerator.GetIdAsString(), id_deparature, amount);
            var user = GetUserByUsername(username).Result;

            //Napravljeno Za Mail Service
            if (user == null)
            {
                return "Ne Postoji Korisnik Sa Tim Korisnickim Imenom!";

            }

            string ErrorMsg = PurchaseCheck(user.AccountNumber, id_deparature, amount);
            if (ErrorMsg != "")
            {
                return ErrorMsg;
            }

            await UpdateBankAccount(user.AccountNumber, id_deparature, amount, "TakeMoney");
            await UpdateDeparature(id_deparature, amount, "TakeTickets");
            await UpdateUser(user, p.Id, "AddPurchase");



            purchaseDict = await stateManager.GetOrAddAsync<IReliableDictionary<string, Purchase>>("PurchaseActiveData");
            using (var tx = stateManager.CreateTransaction())
            {
                result = await purchaseDict.TryAddAsync(tx, p.Id, p);
                await tx.CommitAsync();
            }


            if (result == false)
            {
                return "Kupovina NIJE Dodata";
            }

            List<Purchase> purchases = await PurchaseGetAllData();
            FabricClient fabricClient = new System.Fabric.FabricClient();
            int partitionsNumber = (await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/SistemZaKupovinuKarata/PublisherSubscriber"))).Count;
            int index = 0;

            for (int i = 0; i < partitionsNumber; i++)
            {
                var proxy = ServiceProxy.Create<IPublisherSubscriber>(
                new Uri("fabric:/SistemZaKupovinuKarata/PublisherSubscriber"),
                new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(index % partitionsNumber)
                );

                result = await proxy.PurchasePublishActive(purchases);

                if (result == false)
                {
                    return "Kupovina NIJE Dodata";
                }

                index++;
            }

            return "";
        }

        public async Task<bool> AddBankAccount(BankAccount b)
        {
            var stateManager = this.StateManager;
            bool result = true;

            bankAccountDict = await stateManager.GetOrAddAsync<IReliableDictionary<string, BankAccount>>("BankAccountActiveData");
            using (var tx = stateManager.CreateTransaction())
            {
                result = await bankAccountDict.TryAddAsync(tx, b.Number, b);
                await tx.CommitAsync();
            }

            if (result == false)
            {
                return false;
            }

            List<BankAccount> bankAccounts = await BankAccountGetAllData();
            FabricClient fabricClient = new System.Fabric.FabricClient();
            int partitionsNumber = (await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/SistemZaKupovinuKarata/PublisherSubscriber"))).Count;
            int index = 0;

            for (int i = 0; i < partitionsNumber; i++)
            {
                var proxy = ServiceProxy.Create<IPublisherSubscriber>(
                new Uri("fabric:/SistemZaKupovinuKarata/PublisherSubscriber"),
                new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(index % partitionsNumber)
                );

                result = await proxy.BankAccountPublishActive(bankAccounts);

                if (result == false)
                {
                    return false;
                }

                index++;
            }

            return result;
        }

        public Weather AddWeatherToDeparture(string departurePlace)
        {
            string url = string.Format("https://api.openweathermap.org/data/2.5/weather?q={0}&appid=ee89cb80a57b008a5ca9b94bd300f41b", departurePlace);


            double temperature;
            double windspeed;
            double clouds;
            try
            {
                var client = new WebClient();
                var content = client.DownloadString(url);

                var obj = JsonConvert.DeserializeObject<JObject>(content);

                double tempInKelvin = Double.Parse(obj["main"]["temp"].ToString());
                temperature = Math.Round(tempInKelvin - 273.15, 2);

                windspeed = Double.Parse(obj["wind"]["speed"].ToString());
                clouds = Double.Parse(obj["clouds"]["all"].ToString());

                return new Weather(temperature, clouds, windspeed);
            }
            catch
            {
                ServiceEventSource.Current.Message("Not connected to OpenWeather!");
                return null;
            }
        }

        public async Task<bool> AddDeparature(Departure d)
        {
            var stateManager = this.StateManager;
            bool result = true;

            d.NumberOfAvalableTickets = d.NumberOfTickets;
            d.Weather = AddWeatherToDeparture(d.DeparturePlace);
            if (d.Weather == null)
            {
                return false;
            }

            deparatureDict = await stateManager.GetOrAddAsync<IReliableDictionary<string, Departure>>("DeparatureActiveData");
            using (var tx = stateManager.CreateTransaction())
            {
                result = await deparatureDict.TryAddAsync(tx, d.Id, d);
                await tx.CommitAsync();
            }

            if (result == false)
            {
                return false;
            }

            List<Departure> deparatures = await DeparatureGetAllData();
            FabricClient fabricClient = new System.Fabric.FabricClient();
            int partitionsNumber = (await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/SistemZaKupovinuKarata/PublisherSubscriber"))).Count;
            int index = 0;

            for (int i = 0; i < partitionsNumber; i++)
            {
                var proxy = ServiceProxy.Create<IPublisherSubscriber>(
                new Uri("fabric:/SistemZaKupovinuKarata/PublisherSubscriber"),
                new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(index % partitionsNumber)
                );

                result = await proxy.DeparaturePublishActive(deparatures);

                if (result == false)
                {
                    return false;
                }

                index++;
            }

            return result;
        }

        public async Task<bool> AddUser(User u)
        {
            var stateManager = this.StateManager;
            bool result = true;

            u.PurchaseList = new List<string>();

            result = await BankAccountExists(u.AccountNumber);
            if (result == false)
            {
                return false;
            }

            userDict = await stateManager.GetOrAddAsync<IReliableDictionary<string, User>>("UserActiveData");
            using (var tx = stateManager.CreateTransaction())
            {
                result = await userDict.TryAddAsync(tx, u.Username, u);
                await tx.CommitAsync();
            }

            if (result == false)
            {
                return false;
            }

            List<User> users = await UserGetAllData();
            FabricClient fabricClient = new System.Fabric.FabricClient();
            int partitionsNumber = (await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/SistemZaKupovinuKarata/PublisherSubscriber"))).Count;
            int index = 0;

            for (int i = 0; i < partitionsNumber; i++)
            {
                var proxy = ServiceProxy.Create<IPublisherSubscriber>(
                new Uri("fabric:/SistemZaKupovinuKarata/PublisherSubscriber"),
                new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(index % partitionsNumber)
                );

                bool tempPublish = await proxy.UserPublishActive(users);

                if (tempPublish == false)
                {
                    return tempPublish;
                }

                index++;
            }

            return result;
        }
        #endregion

        #region Get
        public async Task<List<BankAccount>> BankAccountGetAllData()
        {
            var stateManager = this.StateManager;

            List<BankAccount> bankAccounts = new List<BankAccount>();
            bankAccountDict = await stateManager.GetOrAddAsync<IReliableDictionary<string, BankAccount>>("BankAccountActiveData");
            using (var tx = stateManager.CreateTransaction())
            {
                var enumerator = (await bankAccountDict.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
                while (await enumerator.MoveNextAsync(new System.Threading.CancellationToken()))
                {
                    bankAccounts.Add(enumerator.Current.Value);
                }
            }

            return bankAccounts;
        }

        public async Task<List<Departure>> DeparatureGetAllData()
        {
            var stateManager = this.StateManager;

            List<Departure> deparatures = new List<Departure>();
            deparatureDict = await stateManager.GetOrAddAsync<IReliableDictionary<string, Departure>>("DeparatureActiveData");
            using (var tx = stateManager.CreateTransaction())
            {
                var enumerator = (await deparatureDict.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
                while (await enumerator.MoveNextAsync(new System.Threading.CancellationToken()))
                {
                    deparatures.Add(enumerator.Current.Value);
                }
            }

            return deparatures;
        }

        public async Task<List<User>> UserGetAllData()
        {
            var stateManager = this.StateManager;

            List<User> users = new List<User>();
            userDict = await stateManager.GetOrAddAsync<IReliableDictionary<string, User>>("UserActiveData");
            using (var tx = stateManager.CreateTransaction())
            {
                var enumerator = (await userDict.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
                while (await enumerator.MoveNextAsync(new System.Threading.CancellationToken()))
                {
                    users.Add(enumerator.Current.Value);
                }
            }

            return users;
        }

        public async Task<List<Purchase>> PurchaseGetAllData()
        {
            var stateManager = this.StateManager;

            List<Purchase> purchases = new List<Purchase>();
            purchaseDict = await stateManager.GetOrAddAsync<IReliableDictionary<string, Purchase>>("PurchaseActiveData");
            using (var tx = stateManager.CreateTransaction())
            {
                var enumerator = (await purchaseDict.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
                while (await enumerator.MoveNextAsync(new System.Threading.CancellationToken()))
                {
                    purchases.Add(enumerator.Current.Value);
                }
            }

            return purchases;
        }

        #endregion


        public async Task<Purchase> GetPurchaseById(string id_Purchase)
        {
            var stateManager = this.StateManager;

            purchaseDict = await stateManager.GetOrAddAsync<IReliableDictionary<string, Purchase>>("PurchaseActiveData");
            using (var tx = stateManager.CreateTransaction())
            {
                var purchase = await purchaseDict.TryGetValueAsync(tx, id_Purchase);
                return purchase.Value;
            }
        }

        public async Task<string> DeletePurchase(string username, string id_Purchase)
        {
            var stateManager = this.StateManager;

            var user = GetUserByUsername(username).Result;
            var purchase = GetPurchaseById(id_Purchase).Result;
            var departure = GetDepartureById(purchase.DepartureId).Result;

            if (DateTime.Now.AddDays(5) >= DateTime.Parse(departure.DepartureDate))
            {
                return "Zakasnili Ste Sa Otkazivanjem Putovanja!";
            }

            await UpdateBankAccount(user.AccountNumber, purchase.DepartureId, purchase.TicketAmount, "AddMoney");
            await UpdateDeparature(purchase.DepartureId, purchase.TicketAmount, "AddTickets");
            await UpdateUser(user, purchase.Id, "RemovePurchase");


            purchaseDict = await stateManager.GetOrAddAsync<IReliableDictionary<string, Purchase>>("PurchaseActiveData");
            using (var tx = stateManager.CreateTransaction())
            {
                await purchaseDict.TryRemoveAsync(tx, id_Purchase);
                await tx.CommitAsync();
            }

            List<Purchase> purchases = await PurchaseGetAllData();
            FabricClient fabricClient = new System.Fabric.FabricClient();
            int partitionsNumber = (await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/SistemZaKupovinuKarata/PublisherSubscriber"))).Count;
            int index = 0;

            for (int i = 0; i < partitionsNumber; i++)
            {
                var proxy = ServiceProxy.Create<IPublisherSubscriber>(
                new Uri("fabric:/SistemZaKupovinuKarata/PublisherSubscriber"),
                new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(index % partitionsNumber)
                );

                bool tempPublish = await proxy.PurchasePublishActive(purchases);

                if (tempPublish == false)
                {
                    return "Problem Sa Pub/Sub-om";
                }

                index++;
            }

            return "";
        }


        public async Task<bool> BankAccountExists(string accountNumber)
        {
            List<BankAccount> bankAccounts = await BankAccountGetAllData();
            return bankAccounts.Exists(x => x.Number == accountNumber);
        }
       
        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            //izmenjeno
            return this.CreateServiceRemotingReplicaListeners();
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("myDictionary");

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using (var tx = this.StateManager.CreateTransaction())
                {
                    var result = await myDictionary.TryGetValueAsync(tx, "Counter");

                    ServiceEventSource.Current.ServiceMessage(this.Context, "Current Counter Value: {0}",
                        result.HasValue ? result.Value.ToString() : "Value does not exist.");

                    await myDictionary.AddOrUpdateAsync(tx, "Counter", 0, (key, value) => ++value);

                    // If an exception is thrown before calling CommitAsync, the transaction aborts, all changes are 
                    // discarded, and nothing is saved to the secondary replicas.
                    await tx.CommitAsync();
                }

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }

      
    }
}
