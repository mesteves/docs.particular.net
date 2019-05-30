﻿using System;
using System.Threading.Tasks;
using NServiceBus;

class Program
{
    static async Task Main()
    {
        Console.Title = "Samples.MongoDB.Server";

        #region mongoDbConfig

        var endpointConfiguration = new EndpointConfiguration("Samples.MongoDB.Server");
        endpointConfiguration.UsePersistence<MongoDBPersistence>();

        #endregion

        endpointConfiguration.EnableInstallers();
        endpointConfiguration.UseTransport<LearningTransport>();

        var endpointInstance = await Endpoint.Start(endpointConfiguration)
            .ConfigureAwait(false);
        Console.WriteLine("Press any key to exit");
        Console.ReadKey();
        await endpointInstance.Stop()
            .ConfigureAwait(false);
    }
}