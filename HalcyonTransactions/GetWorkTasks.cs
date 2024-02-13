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
    public class GetWorkTasks
    {
        private readonly IConfiguration _configuration;

        public GetWorkTasks(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [FunctionName("GetWorkTasks")]
        public async Task<IActionResult> Run(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
        {
            try
            {
                var conString = _configuration["AzureWebJobsStorage"];
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                WorkTaskModel RequestObject = JsonConvert.DeserializeObject<WorkTaskModel>(requestBody);

                TableClient client = new TableClient(conString, $"{RequestObject.DeviceName}WorkTask");
                await client.CreateIfNotExistsAsync();
                Pageable<WorkTaskModelTableEntity> entities = client.Query<WorkTaskModelTableEntity>();

                List<WorkTaskModel> WorkTaskList = new List<WorkTaskModel>();

                foreach (var worktask in entities.ToList().Where(t => t.Completed != 1))
                {
                    WorkTaskModel workTask = new WorkTaskModel();

                    workTask.Assignment = worktask.Assignment;
                    workTask.Description = worktask.Description;
                    workTask.Effort = worktask.Effort;
                    workTask.ParentPartitionKey = worktask.ParentPartitionKey;
                    workTask.ParentRowKey = worktask.ParentRowKey;
                    workTask.PartitionKey = worktask.PartitionKey;
                    workTask.Priority = worktask.Priority;
                    workTask.Risk = worktask.Risk;
                    workTask.RowKey = worktask.RowKey;
                    workTask.SendSMS = worktask.SendSMS;
                    workTask.StartDate = worktask.StartDate;
                    workTask.State = worktask.State;
                    workTask.TargetDate = worktask.TargetDate;
                    workTask.Title = worktask.Title;
                    workTask.Completed = worktask.Completed;
                    workTask.DeviceName = worktask.DeviceName;

                    WorkTaskList.Add(workTask);
                }
                return new OkObjectResult(JsonConvert.SerializeObject(WorkTaskList));
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }

        }
    }
}
