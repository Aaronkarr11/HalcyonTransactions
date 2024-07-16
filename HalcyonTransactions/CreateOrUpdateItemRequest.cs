using Azure.Data.Tables;
using HalcyonCore.SharedEntities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Functions.Worker;
using Newtonsoft.Json;

namespace HalcyonTransactions
{
    public class CreateOrUpdateRequestItems
    {
        private readonly IConfiguration _configuration;

        public CreateOrUpdateRequestItems(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Function("CreateOrUpdateRequestItems")]
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
