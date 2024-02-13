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
    public class CreateOrUpdateRequestItems
    {
        private readonly IConfiguration _configuration;

        public CreateOrUpdateRequestItems(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [FunctionName("CreateOrUpdateRequestItems")]
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

                RequestItemsTableEntity requestTableEntity = new RequestItemsTableEntity();
                requestTableEntity.PartitionKey = RequestObject.PartitionKey;
                requestTableEntity.RowKey = RequestObject.RowKey;
                requestTableEntity.Title = RequestObject.Title;
                requestTableEntity.ReasonDescription = RequestObject.ReasonDescription;
                requestTableEntity.DesiredDate = DateTime.SpecifyKind(Convert.ToDateTime(RequestObject.DesiredDate), DateTimeKind.Utc);
                requestTableEntity.IsFulfilled = RequestObject.IsFulfilled;

                result = client.UpsertEntity(requestTableEntity);
                return new OkObjectResult(result.Status);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }

        }
    }
}
