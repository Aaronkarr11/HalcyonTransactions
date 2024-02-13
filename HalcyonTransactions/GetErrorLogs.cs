using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using HalcyonCore.SharedEntities;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Azure.Data.Tables;
using Azure;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace HalcyonTransactions
{
    public class GetErrorLogs
    {
        private readonly IConfiguration _configuration;

        public GetErrorLogs(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [FunctionName("GetErrorLogs")]
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
