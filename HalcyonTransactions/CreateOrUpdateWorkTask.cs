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
using System.Net.Mail;
using System.Text;

namespace HalcyonTransactions
{
    public class CreateOrUpdateWorkTask
    {
        private readonly IConfiguration _configuration;

        public CreateOrUpdateWorkTask(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [FunctionName("CreateOrUpdateWorkTask")]
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

                WorkTaskModelTableEntity requestTableEntity = new WorkTaskModelTableEntity();

                requestTableEntity.Assignment = RequestObject.Assignment;
                requestTableEntity.Description = RequestObject.Description;
                requestTableEntity.Effort = RequestObject.Effort;
                requestTableEntity.ParentPartitionKey = RequestObject.ParentPartitionKey;
                requestTableEntity.ParentRowKey = RequestObject.ParentRowKey;
                requestTableEntity.PartitionKey = RequestObject.PartitionKey;
                requestTableEntity.RowKey = RequestObject.RowKey;
                requestTableEntity.Priority = RequestObject.Priority;
                requestTableEntity.Risk = RequestObject.Risk;
                requestTableEntity.SendSMS = RequestObject.SendSMS;
                requestTableEntity.StartDate = DateTime.SpecifyKind(Convert.ToDateTime(RequestObject.StartDate), DateTimeKind.Utc);
                requestTableEntity.State = RequestObject.State;
                requestTableEntity.TargetDate = DateTime.SpecifyKind(Convert.ToDateTime(RequestObject.TargetDate), DateTimeKind.Utc);
                requestTableEntity.Title = RequestObject.Title;
                requestTableEntity.Completed = RequestObject.Completed;
                requestTableEntity.DeviceName = RequestObject.DeviceName;

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
