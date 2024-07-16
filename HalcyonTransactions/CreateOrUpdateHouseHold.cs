using Azure.Data.Tables;
using HalcyonCore.SharedEntities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace HalcyonTransactions
{
    public class CreateOrUpdateHouseHold
    {
        private readonly IConfiguration _configuration;

        public CreateOrUpdateHouseHold(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Function("CreateOrUpdateHouseHold")]
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

                HouseHoldMemberTableEntity requestTableEntity = new HouseHoldMemberTableEntity();
                requestTableEntity.Name = RequestObject.Name;
                requestTableEntity.Email = RequestObject.Email;
                requestTableEntity.PartitionKey = RequestObject.PartitionKey;
                requestTableEntity.RowKey = RequestObject.RowKey;
                requestTableEntity.PhoneNumber = RequestObject.PhoneNumber;
                requestTableEntity.DeviceName = RequestObject.DeviceName;

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
