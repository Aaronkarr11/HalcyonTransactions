using Azure;
using Azure.Data.Tables;
using HalcyonCore.SharedEntities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Microsoft.Azure.Functions.Worker;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Globalization;
using System.Collections.Concurrent;

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
                WMScheduleTableEntity record = client.GetEntityAsync<WMScheduleTableEntity>(RequestObject.PartitionKey, RequestObject.RowKey).Result;

                WMScheduleModel model = new WMScheduleModel();

                model.RowKey = record.RowKey;
                model.PartitionKey = record.PartitionKey;
                model.DeviceName = record.DeviceName;
                model.StartingDate = Convert.ToDateTime(record.StartingDate).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);

                return new OkObjectResult(JsonConvert.SerializeObject(model));
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }

        }
    }
}
