using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Common;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;

namespace Saver
{
    public class SaverService 
    {
        IReliableDictionary<string, User> userDict;
        IReliableStateManager stateManager;

        public SaverService()
        {

        }

        //Constructor Injection
        public SaverService(IReliableStateManager stateManager)
        {
            this.stateManager = stateManager;
        }

        //public async Task<bool> AddUser(User u)
        //{
        //    bool result = true;

        //    userDict = await stateManager.GetOrAddAsync<IReliableDictionary<string, User>>("UserActiveData");
        //    using (var tx = stateManager.CreateTransaction())
        //    {
        //        result = await userDict.TryAddAsync(tx, u.Username, u);
        //        await tx.CommitAsync();
        //    }


        //    List<User> users = await UserGetAllData();
        //    FabricClient fabricClient = new FabricClient();
        //    int partitionsNumber = (await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/SistemZaKupovinuKarata/PublisherSubscriber"))).Count;
        //    var binding = WcfUtility.CreateTcpClientBinding();
        //    int index = 0;
        //    for (int i = 0; i < partitionsNumber; i++)
        //    {
        //        ServicePartitionClient<WcfCommunicationClient<IPublisherSubscriber>> servicePartitionClient = new ServicePartitionClient<WcfCommunicationClient<IPublisherSubscriber>>(
        //            new WcfCommunicationClientFactory<IPublisherSubscriber>(clientBinding: binding),
        //            new Uri("fabric:/SistemZaKupovinuKarata/PublisherSubscriber"),
        //            new ServicePartitionKey(index % partitionsNumber));
        //        bool tempPublish = await servicePartitionClient.InvokeWithRetryAsync(client => client.Channel.UserPublishActive(users));
        //        index++;
        //    }


        //    return result;
        //}


        public async Task<List<User>> UserGetAllData()
        {
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
    }
}
