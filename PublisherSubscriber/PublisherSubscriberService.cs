using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace PublisherSubscriber
{
    public class PublisherSubscriberService 
    {
        IReliableDictionary<string, User> userActiveData;

        IReliableStateManager stateManager;

        public PublisherSubscriberService(IReliableStateManager stateManager)
        {
            this.stateManager = stateManager;
        }

        public PublisherSubscriberService()
        {

        }

        public async Task<bool> UserPublishActive(List<User> users)
        {
            try
            {
                userActiveData = await stateManager.GetOrAddAsync<IReliableDictionary<string, User>>("UserActiveData");
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
    }
}
