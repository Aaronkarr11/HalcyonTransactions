using Azure.Data.Tables;
using HalcyonCore.SharedEntities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Microsoft.Azure.Functions.Worker;

namespace HalcyonTransactions
{
    public class DeleteOrCompleteWorkTask
    {
        private readonly IConfiguration _configuration;

        public DeleteOrCompleteWorkTask(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Function("DeleteOrCompleteWorkTask")]
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
