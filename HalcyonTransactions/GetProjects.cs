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
    public class GetProjects
    {
        private readonly IConfiguration _configuration;

        public GetProjects(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Function("GetProjects")]
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
