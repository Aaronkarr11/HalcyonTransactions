using Azure.Data.Tables;
using HalcyonCore.SharedEntities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Microsoft.Azure.Functions.Worker;

namespace HalcyonTransactions
{
    public class DeleteHouseHold
    {
        private readonly IConfiguration _configuration;

        public DeleteHouseHold(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Function("DeleteHouseHold")]
        public async Task<IActionResult> Run(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
        {
            try
            {
                var conString = _configuration["AzureWebJobsStorage"];
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                HouseHoldMemberTableEntity RequestObject = JsonConvert.DeserializeObject<HouseHoldMemberTableEntity>(requestBody);

                TableClient client = new TableClient(conString, $"{RequestObject.DeviceName}HouseHold");
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
