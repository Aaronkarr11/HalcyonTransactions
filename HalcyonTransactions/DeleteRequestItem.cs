using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using HalcyonCore.SharedEntities;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage;
using Azure.Data.Tables;
using System.Collections.Concurrent;

namespace HalcyonTransactions
{
    public class DeleteRequestItem
    {
        private readonly IConfiguration _configuration;

        public DeleteRequestItem(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [FunctionName("DeleteRequestItem")]
        public async Task<IActionResult> Run(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
        {
            try
            {
                var conString = _configuration["AzureWebJobsStorage"];
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                RequestItemsTableTemplate RequestObject = JsonConvert.DeserializeObject<RequestItemsTableTemplate>(requestBody);

                TableClient client = new TableClient(conString, $"{RequestObject.DeviceName}RequestItems");
                await client.CreateIfNotExistsAsync();

                Azure.Response result;
                result = client.DeleteEntity(RequestObject.PartitionKey, RequestObject.RowKey);
               

                return new OkObjectResult(result.Status);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }

        }
    }
}
