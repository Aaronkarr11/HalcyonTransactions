using Azure;
using Azure.Data.Tables;
using HalcyonCore.SharedEntities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Microsoft.Azure.Functions.Worker;

namespace HalcyonTransactions
{
    public class GetErrorLogs
    {
        private readonly IConfiguration _configuration;

        public GetErrorLogs(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Function("GetErrorLogs")]
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

                List<ErrorLogModel> ErrorLogList = new List<ErrorLogModel>();

                //ToDO Have ammount be a parameter
                foreach (var error in entities.ToList().Take(100))
                {
                    ErrorLogModel errorRecord = new ErrorLogModel();

                    errorRecord.RowKey = error.RowKey;
                    errorRecord.PartitionKey = error.PartitionKey;
                    errorRecord.Message = error.Message;
                    errorRecord.ClassName = error.ClassName;
                    errorRecord.MethodName = error.MethodName;
                    errorRecord.ErrorDate = error.ErrorDate;

                    ErrorLogList.Add(errorRecord);
                }
                return new OkObjectResult(JsonConvert.SerializeObject(ErrorLogList));
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }

        }
    }
}
