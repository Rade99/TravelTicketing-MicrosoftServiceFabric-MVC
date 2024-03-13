using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace PublisherSubscriber
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class PublisherSubscriber : StatefulService, IPublisherSubscriber
    {
        IReliableDictionary<string, User> userActiveData;
        IReliableDictionary<string, Departure> deparatureActiveData;
        IReliableDictionary<string, BankAccount> bankAccountActiveData;
        IReliableDictionary<string, Purchase> purchaseActiveData;

        public PublisherSubscriber(StatefulServiceContext context)
            : base(context)
        { }

        #region Publish
        public async Task<bool> PurchasePublishActive(List<Purchase> purchases)
        {
            var stateManager = this.StateManager;

            try
            {
                purchaseActiveData = await stateManager.GetOrAddAsync<IReliableDictionary<string, Purchase>>("PurchaseActiveDataForWebApp");
                using (var tx = stateManager.CreateTransaction())
                {
                    var enumerator = (await purchaseActiveData.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
                    while (await enumerator.MoveNextAsync(new System.Threading.CancellationToken()))
                    {
                        await purchaseActiveData.TryRemoveAsync(tx, enumerator.Current.Key);
                    }

                    foreach (Purchase purchase in purchases)
                    {
                        await purchaseActiveData.TryAddAsync(tx, purchase.Id, purchase);
                    }
                    await tx.CommitAsync();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeparaturePublishActive(List<Departure> deparatures)
        {
            var stateManager = this.StateManager;

            try
            {
                deparatureActiveData = await stateManager.GetOrAddAsync<IReliableDictionary<string, Departure>>("DeparatureActiveDataForWebApp");
                using (var tx = stateManager.CreateTransaction())
                {
                    var enumerator = (await deparatureActiveData.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
                    while (await enumerator.MoveNextAsync(new System.Threading.CancellationToken()))
                    {
                        await deparatureActiveData.TryRemoveAsync(tx, enumerator.Current.Key);
                    }

                    foreach (Departure deparature in deparatures)
                    {
                        await deparatureActiveData.TryAddAsync(tx, deparature.Id, deparature);
                    }
                    await tx.CommitAsync();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> BankAccountPublishActive(List<BankAccount> bankAccounts)
        {
            var stateManager = this.StateManager;

            try
            {
                bankAccountActiveData = await stateManager.GetOrAddAsync<IReliableDictionary<string, BankAccount>>("BankAccountActiveDataForWebApp");
                using (var tx = stateManager.CreateTransaction())
                {
                    var enumerator = (await bankAccountActiveData.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
                    while (await enumerator.MoveNextAsync(new System.Threading.CancellationToken()))
                    {
                        await bankAccountActiveData.TryRemoveAsync(tx, enumerator.Current.Key);
                    }

                    foreach (BankAccount bankAccount in bankAccounts)
                    {
                        await bankAccountActiveData.TryAddAsync(tx, bankAccount.Number, bankAccount);
                    }
                    await tx.CommitAsync();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UserPublishActive(List<User> users)
        {
            var stateManager = this.StateManager;

            try
            {
                userActiveData = await stateManager.GetOrAddAsync<IReliableDictionary<string, User>>("UserActiveDataForWebApp");
                using (var tx = stateManager.CreateTransaction())
                {
                    var enumerator = (await userActiveData.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
                    while (await enumerator.MoveNextAsync(new System.Threading.CancellationToken()))
                    {
                        await userActiveData.TryRemoveAsync(tx, enumerator.Current.Key);
                    }

                    foreach (User user in users)
                    {
                        await userActiveData.TryAddAsync(tx, user.Username, user);
                    }
                    await tx.CommitAsync();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region GetData
        public async Task<List<Purchase>> GetPurchaseActiveData()
        {
            List<Purchase> purchases = new List<Purchase>();
            purchaseActiveData = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, Purchase>>("PurchaseActiveDataForWebApp");
            using (var tx = this.StateManager.CreateTransaction())
            {
                var enumerator = (await purchaseActiveData.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
                while (await enumerator.MoveNextAsync(new System.Threading.CancellationToken()))
                {
                    purchases.Add(enumerator.Current.Value);
                }
            }
            return purchases;
        }

        public async Task<List<User>> GetUserActiveData()
        {
            List<User> users = new List<User>();
            userActiveData = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, User>>("UserActiveDataForWebApp");

            using (var tx = this.StateManager.CreateTransaction())
            {
                var enumerator = (await userActiveData.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
                while (await enumerator.MoveNextAsync(new System.Threading.CancellationToken()))
                {
                    users.Add(enumerator.Current.Value);
                }
            }
            return users;
        }

        public async Task<List<Departure>> GetDeparatureActiveData()
        {
            List<Departure> deparatures = new List<Departure>();
            deparatureActiveData = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, Departure>>("DeparatureActiveDataForWebApp");
            using (var tx = this.StateManager.CreateTransaction())
            {
                var enumerator = (await deparatureActiveData.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
                while (await enumerator.MoveNextAsync(new System.Threading.CancellationToken()))
                {
                    deparatures.Add(enumerator.Current.Value);
                }
            }
            return deparatures;
        }

        public async Task<List<BankAccount>> GetBankAccountActiveData()
        {
            List<BankAccount> bankAccounts = new List<BankAccount>();
            bankAccountActiveData = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, BankAccount>>("BankAccountActiveDataForWebApp");

            using (var tx = this.StateManager.CreateTransaction())
            {
                var enumerator = (await bankAccountActiveData.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
                while (await enumerator.MoveNextAsync(new System.Threading.CancellationToken()))
                {
                    bankAccounts.Add(enumerator.Current.Value);
                }
            }
            return bankAccounts;
        }

        public async Task<User> GetUserByUsername(string username)
        {
            var stateManager = this.StateManager;

            userActiveData = await stateManager.GetOrAddAsync<IReliableDictionary<string, User>>("UserActiveDataForWebApp");
            using (var tx = stateManager.CreateTransaction())
            {
                var user = await userActiveData.TryGetValueAsync(tx, username);
                return user.Value;
            }
        }

        public async Task<List<Purchase>> GetUserPurchases(string username)
        {
            User u = GetUserByUsername(username).Result;
            List<Purchase> purchases = new List<Purchase>();

            purchaseActiveData = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, Purchase>>("PurchaseActiveDataForWebApp");
            using (var tx = this.StateManager.CreateTransaction())
            {
                var enumerator = (await purchaseActiveData.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
                while (await enumerator.MoveNextAsync(new System.Threading.CancellationToken()))
                {
                    if (u.PurchaseList.Contains(enumerator.Current.Value.Id))
                    {
                        purchases.Add(enumerator.Current.Value);
                    }
                }
            }
            return purchases;
        }
        #endregion


        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
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
