using Azure;
using Azure.Data.Tables;
using HalcyonCore.SharedEntities;
using HalcyonCore.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Microsoft.Azure.Functions.Worker;

namespace HalcyonTransactions
{
    public class GetDashBoardData
    {
        private readonly IConfiguration _configuration;

        public GetDashBoardData(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Function("GetDashBoardData")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req)
        {
            try
            {
                WorkTaskCompletedStats percentages = new WorkTaskCompletedStats();
                BarGraphModelItem barGraphModelItem = new BarGraphModelItem();
                List<LineGraphModelItem> lineGraphModel = new List<LineGraphModelItem>();

                var conString = _configuration["AzureWebJobsStorage"];
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                WorkTaskModel RequestObject = JsonConvert.DeserializeObject<WorkTaskModel>(requestBody);

                TableClient client = new TableClient(conString, $"{RequestObject.DeviceName}WorkTask");
                await client.CreateIfNotExistsAsync();

                Pageable<WorkTaskModelTableEntity> entites = client.Query<WorkTaskModelTableEntity>();

                var workTaskResult = entites.ToList();

                List<WorkTaskModel> WorkTaskList = new List<WorkTaskModel>();

                DateTime todayOfCurrentMonth = Convert.ToDateTime(DateTime.Now.LastDayInThisMonth());
                DateTime todayOfLastMonth = Convert.ToDateTime(DateTime.Now.AddMonths(-1).LastDayInThisMonth());

                List<WorkTaskModel> workTaskList = new List<WorkTaskModel>();
                List<WorkTaskModelTableTemplate> newResults = new List<WorkTaskModelTableTemplate>();
                // newResults = workTaskResult.Where(o => o.Timestamp >= todayOfLastMonth & o.Timestamp <= todayOfCurrentMonth).ToList();
                foreach (var workTask in workTaskResult.Where(i => (i.Timestamp.Value.LocalDateTime.ToString("yyyy")) == DateTime.Now.Year.ToString()))
                {
                    WorkTaskModel workTaskModel = new WorkTaskModel();
                    workTaskModel.Name = workTask.Assignment;
                    workTaskModel.Completed = workTask.Completed;
                    workTaskList.Add(workTaskModel);
                }
                percentages = CalculatePercents(workTaskList);
                lineGraphModel = ComputeLineGraphSeries(workTaskResult);
                barGraphModelItem = ComputeBarGraphSeries(workTaskResult);

                DashBoard dashBoard = new DashBoard();
                dashBoard.lineGraphModel = lineGraphModel;
                dashBoard.percentageData = percentages;
                dashBoard.barGraphData = barGraphModelItem;

                return new OkObjectResult(dashBoard);

            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }

        }


        public WorkTaskCompletedStats CalculatePercents(List<WorkTaskModel> returnedWorkItems)
        {
            WorkTaskCompletedStats assignmentPercent = new WorkTaskCompletedStats();

            try
            {
                int TotalCounts = returnedWorkItems.Count();
                int TotalCountCompleted = returnedWorkItems.Where(t => t.Completed == 1).Count();
                double completedPercent = Math.Round(((Convert.ToDouble(TotalCountCompleted) / Convert.ToDouble(TotalCounts)) * 100), 0);
                double unCompletedPercent = Convert.ToDouble((100 - completedPercent));
                assignmentPercent.percentCompleted = completedPercent;
                assignmentPercent.percentUnCompleted = unCompletedPercent;
                assignmentPercent.unCompletedCount = TotalCounts;
                assignmentPercent.completedCount = TotalCountCompleted;
                return assignmentPercent;
            }
            catch (Exception)
            {
                return assignmentPercent;
            }

        }

        public List<LineGraphModelItem> ComputeLineGraphSeries(IList<WorkTaskModelTableEntity> workTaskResult)
        {
            List<string> MonthList = new List<string>();
            MonthList.Add("Jan");
            MonthList.Add("Feb");
            MonthList.Add("Mar");
            MonthList.Add("Apr");
            MonthList.Add("May");
            MonthList.Add("Jun");
            MonthList.Add("Jul");
            MonthList.Add("Aug");
            MonthList.Add("Sep");
            MonthList.Add("Oct");
            MonthList.Add("Nov");
            MonthList.Add("Dec");

            List<WorkTaskModel> WorkTaskList = new List<WorkTaskModel>();

            List<LineGraphModelItem> graphModel = new List<LineGraphModelItem>();
            try
            {
                foreach (var month in MonthList)
                {
                    LineGraphModelItem graphModelItem = new LineGraphModelItem();
                    int counter = 0;
                    foreach (var task in workTaskResult.Where(o => o.Completed == 1).Where(i => (i.Timestamp.Value.LocalDateTime.ToString("yyyy")) == DateTime.Now.Year.ToString()).Where(p => p.Timestamp.Value.LocalDateTime.ToString("m").Substring(0, 3) == month))
                    {
                        counter++;
                    }
                    graphModelItem.TotalCompleted = counter;
                    graphModelItem.Name = month;
                    graphModel.Add(graphModelItem);
                }
                return graphModel;
            }
            catch (Exception)
            {

                return graphModel;
            }


        }

        public BarGraphModelItem ComputeBarGraphSeries(IList<WorkTaskModelTableEntity> workTaskResult)
        {
            BarGraphModelItem barGraphModelItem = new BarGraphModelItem();

            try
            {
                List<WorkTaskModelTableEntity> TotalCountCompleted = workTaskResult.Where(o => o.Completed == 1).Where(i => (i.Timestamp.Value.LocalDateTime.ToString("yyyy")) == DateTime.Now.Year.ToString()).ToList();

                barGraphModelItem.CompletedCountForLastMonth = TotalCountCompleted.Where(i => i.Timestamp.Value.LocalDateTime.ToString("MMMM") == DateTime.Now.AddMonths(-1).ToString("MMMM")).Count();
                barGraphModelItem.CompletedCountForCurrentMonth = TotalCountCompleted.Where(i => i.Timestamp.Value.LocalDateTime.ToString("MMMM") == DateTime.Now.ToString("MMMM")).Count();
                barGraphModelItem.CurrentMonth = DateTime.Now.ToString("MMMM");
                barGraphModelItem.LastMonth = DateTime.Now.AddMonths(-1).ToString("MMMM");
                return barGraphModelItem;
            }
            catch (Exception)
            {
                return barGraphModelItem;
            }
        }
    }
}
