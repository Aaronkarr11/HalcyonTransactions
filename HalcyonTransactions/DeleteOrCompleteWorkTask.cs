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
    public class DeleteOrCompleteWorkTask
    {
        private readonly IConfiguration _configuration;

        public DeleteOrCompleteWorkTask(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [FunctionName("DeleteOrCompleteWorkTask")]
        public async Task<IActionResult> Run(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
        {
            try
            {
                var conString = _configuration["AzureWebJobsStorage"];
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                WorkTaskModel RequestObject = JsonConvert.DeserializeObject<WorkTaskModel>(requestBody);

                TableClient client = new TableClient(conString, $"{RequestObject.DeviceName}WorkTask");
                await client.CreateIfNotExistsAsync();

                Azure.Response result;

                if (RequestObject.Completed == 1)
                {
                    WorkTaskModelTableEntity requestTableEntity = new WorkTaskModelTableEntity();
                    requestTableEntity.Completed = 1;
                    requestTableEntity.PartitionKey = RequestObject.PartitionKey;
                    requestTableEntity.RowKey = RequestObject.RowKey;
                    result = client.UpsertEntity(requestTableEntity);
                }
                else
                {
                    result = client.DeleteEntity(RequestObject.PartitionKey, RequestObject.RowKey);
                }

                return new OkObjectResult(result.Status);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }

        }
    }
}
