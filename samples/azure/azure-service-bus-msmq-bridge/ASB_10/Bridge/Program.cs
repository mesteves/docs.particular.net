﻿using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Configuration.AdvancedExtensibility;
using NServiceBus.Router;
using NServiceBus.Serialization;
using SettingsHolder = NServiceBus.Settings.SettingsHolder;

class Program
{
    static async Task Main(string[] args)
    {
        Console.Title = "Samples.Azure.ServiceBus.Bridge";

        var connectionString = Environment.GetEnvironmentVariable("AzureServiceBus.ConnectionString");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new Exception("Could not read the 'AzureServiceBus.ConnectionString' environment variable. Check the sample prerequisites.");
        }

        #region bridge-general-configuration

        var bridgeConfiguration = new RouterConfiguration("Bridge");
#pragma warning disable 618
        var azureInterface = bridgeConfiguration.AddInterface<AzureServiceBusTransport>("ASB", transport =>
#pragma warning restore 618
        {
            //Prevents ASB from using TransactionScope
            transport.Transactions(TransportTransactionMode.ReceiveOnly);
            transport.ConnectionString(connectionString);

            // TODO: ASB requires serializer to be registered.
            // Currently, there's no way to specify serialization for the bridged endpoints
            // endpointConfiguration.UseSerialization<T>();
            var settings = transport.GetSettings();
            var serializer = Tuple.Create(new NewtonsoftSerializer() as SerializationDefinition, new SettingsHolder());
            settings.Set("MainSerializer", serializer);

            var topology = transport.UseEndpointOrientedTopology().EnableMigrationToForwardingTopology();
            topology.RegisterPublisher(typeof(OtherEvent), "Samples.Azure.ServiceBus.AsbEndpoint");
        });
        var msmqInterface = bridgeConfiguration.AddInterface<MsmqTransport>("MSQM", transport => { });
        msmqInterface.EnableMessageDrivenPublishSubscribe(new InMemorySubscriptionStorage());

        bridgeConfiguration.AutoCreateQueues();

        var staticRouting = bridgeConfiguration.UseStaticRoutingProtocol();

        staticRouting.AddForwardRoute(
            incomingInterface: "MSQM",
            outgoingInterface: "ASB");

        staticRouting.AddForwardRoute(
            incomingInterface: "ASB",
            outgoingInterface: "MSQM");

        #endregion

        #region bridge-execution

        var bridge = Router.Create(bridgeConfiguration);

        await bridge.Start().ConfigureAwait(false);

        Console.WriteLine("Press any key to exit");
        Console.ReadKey();

        await bridge.Stop().ConfigureAwait(false);


        #endregion
    }
}