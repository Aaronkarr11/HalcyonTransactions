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
    public class CreateOrUpdateErrorLog
    {
        private readonly IConfiguration _configuration;

        public CreateOrUpdateErrorLog(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Function("CreateOrUpdateErrorLog")]
        public async Task<IActionResult> Run(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
        {
            try
            {
                var conString = _configuration["AzureWebJobsStorage"];
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                ErrorLogModel RequestObject = JsonConvert.DeserializeObject<ErrorLogModel>(requestBody);

                TableClient client = new TableClient(conString, $"{RequestObject.DeviceName}ErrorLogs");
                await client.CreateIfNotExistsAsync();
                Pageable<ErrorLogTableEntity> entities = client.Query<ErrorLogTableEntity>();

                ErrorLogTableEntity requestTableEntity = new ErrorLogTableEntity();

                requestTableEntity.RowKey = RequestObject.RowKey;
                requestTableEntity.PartitionKey = RequestObject.PartitionKey;
                requestTableEntity.Message = RequestObject.Message;
                requestTableEntity.ClassName = RequestObject.ClassName;
                requestTableEntity.MethodName = RequestObject.MethodName;
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
