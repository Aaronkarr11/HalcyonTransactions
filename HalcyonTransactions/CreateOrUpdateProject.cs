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
    public class CreateOrUpdateProject
    {
        private readonly IConfiguration _configuration;

        public CreateOrUpdateProject(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Function("CreateOrUpdateProject")]
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

                ProjectModelTableEntity requestTableEntity = new ProjectModelTableEntity();

                requestTableEntity.RowKey = RequestObject.RowKey;
                requestTableEntity.PartitionKey = RequestObject.PartitionKey;
                requestTableEntity.Title = RequestObject.Title;
                requestTableEntity.Description = RequestObject.Description;
                requestTableEntity.State = RequestObject.State;
                requestTableEntity.StartDate = DateTime.SpecifyKind(Convert.ToDateTime(RequestObject.StartDate), DateTimeKind.Utc);
                requestTableEntity.TargetDate = DateTime.SpecifyKind(Convert.ToDateTime(RequestObject.TargetDate), DateTimeKind.Utc);
                requestTableEntity.CreatedDate = DateTime.SpecifyKind(Convert.ToDateTime(DateTime.Now), DateTimeKind.Utc);
                requestTableEntity.ConvertedDateTimeStamp = Convert.ToInt64(Convert.ToDateTime(requestTableEntity.CreatedDate).ToString("yyyyMMddHHmmss"));
                requestTableEntity.LocationCategory = RequestObject.LocationCategory;
                requestTableEntity.Priority = RequestObject.Priority;
                requestTableEntity.Severity = RequestObject.Severity;

                var result = client.UpsertEntity(requestTableEntity);
                return new OkObjectResult(JsonConvert.SerializeObject(result.Status));
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }

        }


        //TODO: Add SMS or push notf.
        //private void SendSMS(WorkTaskModel workTaskModel)
        //{
        //    try
        //    {
        //        if (!String.IsNullOrEmpty(workTaskModel.PhoneNumber))
        //        {
        //            string phoneToSend = "+1" + workTaskModel.PhoneNumber.RemoveSpecialCharacters().Trim();
        //            var assignee = workTaskModel.Name;

        //            var title = workTaskModel.Title;

        //            var endDate = workTaskModel.TargetDate.ToString().Split(" ")[0];

        //            var messageToSend = $"Yay {assignee}! You have a new Work Item, '{title}', assigned to you. Target date is {endDate}. Reply STOP to stop receiving messages";

        //            string connectionString = _configuration["HalcyonSMS"];
        //            SmsClient smsClient = new SmsClient(connectionString);

        //            SmsSendResult sendResult = smsClient.Send(
        //            from: _configuration["HalcyonNum"],
        //            to: phoneToSend,
        //            message: messageToSend
        //            );
        //        }
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //}

        //public string RemoveSpecialCharacters(string str)
        //{
        //    try
        //    {
        //        StringBuilder sb = new StringBuilder();
        //        foreach (char c in str)
        //        {
        //            if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_')
        //            {
        //                sb.Append(c);
        //            }
        //        }
        //        string appendedString = sb.ToString();
        //        appendedString.Replace(" ", "");
        //        return appendedString;
        //    }
        //    catch (Exception)
        //    {
        //        return "Pog";
        //    }

        //}
    }
}
