using Azure;
using Azure.Data.Tables;
using HalcyonCore.SharedEntities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace HalcyonTransactions
{
    public class CreateOrUpdateWMSchedule
    {
        private readonly IConfiguration _configuration;

        public CreateOrUpdateWMSchedule(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Function("CreateOrUpdateWMSchedule")]
        public async Task<IActionResult> Run(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
        {
            try
            {
                var conString = _configuration["AzureWebJobsStorage"];
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                WMScheduleModel RequestObject = JsonConvert.DeserializeObject<WMScheduleModel>(requestBody);

                TableClient client = new TableClient(conString, $"{RequestObject.DeviceName}WMSchedule");
                await client.CreateIfNotExistsAsync();
                Pageable<WMScheduleTableEntity> entities = client.Query<WMScheduleTableEntity>();

                WMScheduleTableEntity requestTableEntity = new WMScheduleTableEntity();

                requestTableEntity.RowKey = RequestObject.RowKey;
                requestTableEntity.PartitionKey = RequestObject.PartitionKey;
                requestTableEntity.DeviceName = RequestObject.DeviceName;
                requestTableEntity.StartingDate = RequestObject.StartingDate;
                requestTableEntity.Timestamp = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);

                var result = client.UpsertEntity(requestTableEntity);
                return new OkObjectResult(JsonConvert.SerializeObject(result.Status));
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }
        }
    }
}
