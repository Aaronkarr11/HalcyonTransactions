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
    public class GetProjects
    {
        private readonly IConfiguration _configuration;

        public GetProjects(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [FunctionName("GetProjects")]
        public async Task<IActionResult> Run(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
        {
            try
            {
                var conString = _configuration["AzureWebJobsStorage"];
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                ProjectModel RequestObject = JsonConvert.DeserializeObject<ProjectModel>(requestBody);

                TableClient client = new TableClient(conString, $"{RequestObject.DeviceName}Project");
                await client.CreateIfNotExistsAsync();
                Pageable<ProjectModelTableEntity> entities = client.Query<ProjectModelTableEntity>();

                List<ProjectModel> ProjectList = new List<ProjectModel>();

                foreach (var project in entities.ToList().Where(t => t.Completed != 1))
                {
                    ProjectModel projectRecord = new ProjectModel();

                    projectRecord.Description = project.Description;
                    projectRecord.PartitionKey = project.PartitionKey;
                    projectRecord.Priority = project.Priority;
                    projectRecord.RowKey = project.RowKey;
                    projectRecord.StartDate = project.StartDate;
                    projectRecord.State = project.State;
                    projectRecord.TargetDate = project.TargetDate;
                    projectRecord.CreatedDate = project.CreatedDate;
                    projectRecord.ConvertedDateTimeStamp = project.ConvertedDateTimeStamp;
                    projectRecord.Title = project.Title;
                    projectRecord.Completed = project.Completed;
                    projectRecord.DeviceName = project.DeviceName;

                    ProjectList.Add(projectRecord);
                }
                return new OkObjectResult(JsonConvert.SerializeObject(ProjectList));
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }

        }
    }
}
