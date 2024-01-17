#r "Microsoft.WindowsAzure.Storage"
#r "Microsoft.Azure.DocumentDB.Core"

using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.Azure.Documents.Client;

public static class BlobToCosmosFunction
{
    [FunctionName("BlobToCosmosFunction")]
    public static async Task<IActionResult> Run(
        [BlobTrigger("sdvdemocontainer/{name}", Connection = "AzureWebJobsStorage")] Stream blobStream,
        [CosmosDB(
            databaseName: "YourCosmosDBDatabase",
            collectionName: "YourCosmosDBCollection",
            ConnectionStringSetting = "CosmosDBConnectionString")] IAsyncCollector<dynamic> documentsOut,
        string name,
        ILogger log)
    {
        log.LogInformation($"C# Blob trigger function processed blob\n Name:{name} \n Size: {blobStream.Length} Bytes");

        try
        {
            // Lesen Sie den Inhalt der Blob-Datei
            using (var reader = new StreamReader(blobStream))
            {
                string content = await reader.ReadToEndAsync();

                // Erstellen Sie ein JSON-Dokument, das in Cosmos DB hochgeladen wird
                var document = new
                {
                    id = Guid.NewGuid().ToString(),
                    filename = name,
                    content = content
                };

                // Hochladen des Dokuments in Cosmos DB
                await documentsOut.AddAsync(document);
            }

            return new OkObjectResult($"Datei {name} wurde erfolgreich in Cosmos DB hochgeladen.");
        }
        catch (Exception ex)
        {
            log.LogError($"Fehler beim Hochladen der Datei {name} in Cosmos DB: {ex.Message}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}
