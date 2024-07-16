using Azure.Data.Tables;
using HalcyonCore.SharedEntities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace HalcyonTransactions
{
    public class DeleteRequestItem
    {
        private readonly IConfiguration _configuration;

        public DeleteRequestItem(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Function("DeleteRequestItem")]
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
