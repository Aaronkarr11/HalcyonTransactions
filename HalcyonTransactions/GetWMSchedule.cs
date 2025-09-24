using Azure;
using Azure.Data.Tables;
using HalcyonCore.SharedEntities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Microsoft.Azure.Functions.Worker;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HalcyonTransactions
{
    public class GetWMSchedule
    {
        private readonly IConfiguration _configuration;

        public GetWMSchedule(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Function("GetWMSchedule")]
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

                WMScheduleModel model = new WMScheduleModel();

                model.RowKey = RequestObject.RowKey;
                model.PartitionKey = RequestObject.PartitionKey;
                model.DeviceName = RequestObject.DeviceName;
                model.StartingDate = RequestObject.StartingDate;

                return new OkObjectResult(JsonConvert.SerializeObject(RequestObject));
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }

        }
    }
}
