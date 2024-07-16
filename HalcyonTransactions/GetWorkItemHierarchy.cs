using Azure;
using Azure.Data.Tables;
using HalcyonCore.SharedEntities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Microsoft.Azure.Functions.Worker;
using System.Globalization;

namespace HalcyonTransactions
{
    public class GetWorkItemHierarchy
    {
        private readonly IConfiguration _configuration;

        public GetWorkItemHierarchy(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Function("GetWorkItemHierarchy")]
        public async Task<IActionResult> Run(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
        {
            try
            {
                var conString = _configuration["AzureWebJobsStorage"];
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                ProjectModel RequestObject = JsonConvert.DeserializeObject<ProjectModel>(requestBody);

                TableClient projectClient = new TableClient(conString, $"{RequestObject.DeviceName}Project");
                await projectClient.CreateIfNotExistsAsync();
                Pageable<ProjectModelTableEntity> projectEntities = projectClient.Query<ProjectModelTableEntity>();

                TableClient workTaskClient = new TableClient(conString, $"{RequestObject.DeviceName}WorkTask");
                await workTaskClient.CreateIfNotExistsAsync();
                Pageable<WorkTaskModelTableEntity> workTaskEntities = workTaskClient.Query<WorkTaskModelTableEntity>();

                List<ProjectHierarchy> ProjectList = new List<ProjectHierarchy>();
                List<WorkTaskModel> WorkTaskList = new List<WorkTaskModel>();


                foreach (var project in projectEntities.ToList())
                {
                    ProjectHierarchy projectHierarchy = new ProjectHierarchy();

                    projectHierarchy.WorkTaskHierarchy = new List<WorkTaskModel>();
                    projectHierarchy.Description = project.Description;
                    projectHierarchy.LocationCategory = project.LocationCategory;
                    projectHierarchy.PartitionKey = project.PartitionKey;
                    projectHierarchy.RowKey = project.RowKey;
                    projectHierarchy.Priority = project.Priority;
                    projectHierarchy.Severity = project.Severity;
                    projectHierarchy.StartDate = project.StartDate;
                    projectHierarchy.TargetDate = project.TargetDate;
                    projectHierarchy.CreatedDate = project.CreatedDate;
                    projectHierarchy.DisplayStartDate = Convert.ToDateTime(project.StartDate).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    projectHierarchy.DisplayTargetDate = Convert.ToDateTime(project.TargetDate).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    projectHierarchy.State = project.State;
                    projectHierarchy.ConvertedDateTimeStamp = project.ConvertedDateTimeStamp;
                    projectHierarchy.Title = project.Title;
                    projectHierarchy.Completed = project.Completed;
                    projectHierarchy.DeviceName = project.DeviceName;

                    ProjectList.Add(projectHierarchy);
                    foreach (var worktask in workTaskEntities.ToList().Where(t => t.ParentPartitionKey == project.PartitionKey && t.ParentRowKey == project.RowKey && t.Completed != 1))
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
                        workTask.DisplayStartDate = Convert.ToDateTime(worktask.StartDate).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                        workTask.DisplayTargetDate = Convert.ToDateTime(worktask.TargetDate).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                        workTask.StartDate = worktask.StartDate;
                        workTask.State = worktask.State;
                        workTask.StateColor = Helpers.SetStateColor(worktask.State);
                        workTask.TargetDate = worktask.TargetDate;
                        workTask.Title = worktask.Title;
                        workTask.Completed = worktask.Completed;
                        workTask.DeviceName = worktask.DeviceName;

                        projectHierarchy.WorkTaskHierarchy.Add(workTask);
                    }
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
