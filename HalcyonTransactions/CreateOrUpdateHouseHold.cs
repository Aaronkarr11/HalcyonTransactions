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
    public class CreateOrUpdateHouseHold
    {
        private readonly IConfiguration _configuration;

        public CreateOrUpdateHouseHold(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [FunctionName("CreateOrUpdateHouseHold")]
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
