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
    public class GetOperations
    {
        private readonly IConfiguration _configuration;

        public GetOperations(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Function("GetOperations")]
        public async Task<IActionResult> Run(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
        {
            try
            {
                var conString = _configuration["AzureWebJobsStorage"];
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                OperationModel RequestObject = JsonConvert.DeserializeObject<OperationModel>(requestBody);

                TableClient client = new TableClient(conString, $"{RequestObject.DeviceName}Operation");
                await client.CreateIfNotExistsAsync();

                Pageable<OperationModelTableEntity> operationResult = client.Query<OperationModelTableEntity>();

                List<WorkTaskModel> WorkTaskList = new List<WorkTaskModel>();

                List<OperationModel> OperationList = new List<OperationModel>();

                foreach (var operation in operationResult.ToList())
                {
                    OperationModel operationModel = new OperationModel();

                    operationModel.Description = operation.Description;
                    operationModel.Icon = operation.Icon;
                    operationModel.PartitionKey = operation.PartitionKey;
                    operationModel.RowKey = operation.RowKey;
                    operationModel.StartDate = operation.StartDate;
                    operationModel.TargetDate = operation.TargetDate;
                    operationModel.Title = operation.Title;
                    operationModel.DeviceName = operationModel.DeviceName;
                    OperationList.Add(operationModel);
                }
                return new OkObjectResult(JsonConvert.SerializeObject(OperationList));
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }

        }
    }
}
