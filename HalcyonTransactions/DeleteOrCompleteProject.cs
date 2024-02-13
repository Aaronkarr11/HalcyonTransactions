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
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage;
using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
using Azure;
using System.Linq;

namespace HalcyonTransactions
{
    public class DeleteOrCompleteProject
    {
        private readonly IConfiguration _configuration;

        public DeleteOrCompleteProject(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [FunctionName("DeleteOrCompleteProject")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req)
        {

            try
            {
                var conString = _configuration["AzureWebJobsStorage"];
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                ProjectModel RequestObject = JsonConvert.DeserializeObject<ProjectModel>(requestBody);

                TableClient projectClient = new TableClient(conString, $"{RequestObject.DeviceName}Project");
                await projectClient.CreateIfNotExistsAsync();

                Azure.Response projectResult;

                if (RequestObject.Completed == 1)
                {
                    ProjectModelTableEntity requestTableEntity = new ProjectModelTableEntity();
                    requestTableEntity.Completed = 1;
                    requestTableEntity.PartitionKey = RequestObject.PartitionKey;
                    requestTableEntity.RowKey = RequestObject.RowKey;
                    projectResult = projectClient.UpsertEntity(requestTableEntity);
                }
                else
                {
                    projectResult = projectClient.DeleteEntity(RequestObject.PartitionKey, RequestObject.RowKey);
                }

                TableClient workTaskClient = new TableClient(conString, $"{RequestObject.DeviceName}WorkTask");
                await workTaskClient.CreateIfNotExistsAsync();
                Pageable<WorkTaskModelTableEntity> workTaskEntities = workTaskClient.Query<WorkTaskModelTableEntity>();

                foreach (var workTask in workTaskEntities.ToList().Where(t => t.ParentPartitionKey == RequestObject.PartitionKey && t.ParentRowKey == RequestObject.RowKey))
                {

                    if (RequestObject.Completed == 1)
                    {
                        WorkTaskModelTableEntity requestTableEntity = new WorkTaskModelTableEntity();
                        requestTableEntity.Completed = 1;
                        requestTableEntity.PartitionKey = workTask.PartitionKey;
                        requestTableEntity.RowKey = workTask.RowKey;
                        workTaskClient.UpsertEntity(requestTableEntity);
                    }
                    else
                    {
                        workTaskClient.DeleteEntity(workTask.PartitionKey, workTask.RowKey);
                    }
                }

                return new OkObjectResult(projectResult.Status);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }


        }
    }
}
