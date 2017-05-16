﻿using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using System.Fabric.Description;
using System.Text;

namespace AdminService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class AdminService : StatefulService, IAdminService
    {
        private const string KEY1 = "key1";
        private const string KEY2 = "key2";
        private const string COLLECTION_KEYS = "keys";
        private readonly FabricClient fabric;
        private readonly string applicationName;

        public AdminService(StatefulServiceContext context)
            : base(context)
        {
            fabric = new FabricClient();
            applicationName = this.Context.CodePackageActivationContext.ApplicationName;
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
            return new List<ServiceReplicaListener>()
            {
                new ServiceReplicaListener(
                    (context) =>
                        this.CreateServiceRemotingListener(context))
            };
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // Generate keys only once
            // have to check first to see if it's already been initialized
            // because RunAsync will run multiple times throughout a service's lifetime 
            await GenerateServiceKeys();
        }

        public async Task<string> GetKey1()
        {
            return await GetKey(KEY1);
        }
            
        public async Task<string> GetKey2()
        {
            return await GetKey(KEY2);
        }

        public Task CreateNewTopic(string topicName)
        {
            string applicationName = this.Context.CodePackageActivationContext.ApplicationName;

            StatefulServiceDescription serviceDescription = new StatefulServiceDescription()
            {
                ApplicationName = new Uri(applicationName),
                MinReplicaSetSize = 3,
                TargetReplicaSetSize = 3,
                PartitionSchemeDescription = new UniformInt64RangePartitionSchemeDescription()
                {
                    LowKey = 0,
                    HighKey = 10,
                    PartitionCount = 1
                },
                HasPersistedState = true,
                //InitializationData = Encoding.UTF8.GetBytes(parameters),
                ServiceTypeName = "TopicServiceType",
                ServiceName = CreateTopicUri(topicName)
            };

            return fabric.ServiceManager.CreateServiceAsync(serviceDescription);         
        }

        public Task DeleteTopic(string topicName)
        {
            Uri serviceUri = this.CreateTopicUri(topicName);

            var description = new DeleteServiceDescription(serviceUri);

            return fabric.ServiceManager.DeleteServiceAsync(description);
        }

        private async Task<string> GetKey(string keyName)
        {
            var topics = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, string>>(COLLECTION_KEYS);

            using (var tx = this.StateManager.CreateTransaction())
            {
                var key = await topics.TryGetValueAsync(tx, keyName);
                if (key.HasValue)
                {
                    return key.Value;
                }

                throw new Exception("No Key initialized.");
            }
        }

        private async Task GenerateServiceKeys()
        {
            var topics = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, string>>(COLLECTION_KEYS);

            using (var tx = this.StateManager.CreateTransaction())
            {
                var key1 = await topics.TryGetValueAsync(tx, KEY1);

                var isKey1Initialized = key1.HasValue && !string.IsNullOrWhiteSpace(key1.Value);
                if (!isKey1Initialized)
                {
                    //TODO generator a valid security key. using basic placeholder for now
                    await topics.TryAddAsync(tx, KEY1, Guid.NewGuid().ToString());
                }

                var key2 = await topics.TryGetValueAsync(tx, KEY2);

                var isKey2Initialized = key2.HasValue && !string.IsNullOrWhiteSpace(key2.Value);
                if (!isKey2Initialized)
                {
                    //TODO generator a valid security key. using basic placeholder for now
                    await topics.TryAddAsync(tx, KEY2, Guid.NewGuid().ToString());
                }

                await tx.CommitAsync();
            } 
        }

        private Uri CreateTopicUri(string topicName)
        {
            return new Uri($"{this.applicationName}/topics/{topicName}");
        }

        public Task RegenerateKeys()
        {
            throw new NotImplementedException();
        }
    }
}