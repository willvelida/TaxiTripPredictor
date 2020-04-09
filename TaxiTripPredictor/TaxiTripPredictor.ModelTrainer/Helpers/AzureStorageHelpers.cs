using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace TaxiTripPredictor.ModelTrainer.Helpers
{
    public static class AzureStorageHelpers
    {   
        public static CloudBlobClient ConnectToBlobClient(string accountName, string accountKey)
        {
            try
            {
                StorageCredentials storageCredentials = new StorageCredentials(accountName, accountKey);
                CloudStorageAccount cloudStorageAccount = new CloudStorageAccount(storageCredentials, true);
                CloudBlobClient blobClient = cloudStorageAccount.CreateCloudBlobClient();

                return blobClient;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Something went wrong. Exception thrown: {ex.Message}");
                throw;
            }
        }

        public static CloudBlobContainer GetBlobContainer(CloudBlobClient blobClient, string blobName)
        {
            try
            {
                CloudBlobContainer container = blobClient.GetContainerReference(blobName);
                return container;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Something went wrong. Exception thrown: {ex.Message}");
                throw;
            }
        }

        public static async Task UploadBlobToStorage(CloudBlobContainer container, string blobName)
        {
            CloudBlockBlob cloudBlockBlob = container.GetBlockBlobReference(blobName);
            await cloudBlockBlob.UploadFromFileAsync(blobName);
        }

        public static async Task DownloadBlobAsync(CloudBlobContainer container, string blobName)
        {
            CloudBlockBlob cloudBlockBlob = container.GetBlockBlobReference(blobName);
            await cloudBlockBlob.DownloadToFileAsync(blobName, FileMode.Open);
        }
    }
}
