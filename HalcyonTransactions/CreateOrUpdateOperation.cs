using Azure;
using Azure.Data.Tables;
using HalcyonCore.SharedEntities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Functions.Worker;
using Newtonsoft.Json;

namespace HalcyonTransactions
{
    public class CreateOrUpdateOperation
    {
        private readonly IConfiguration _configuration;

        public CreateOrUpdateOperation(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Function("CreateOrUpdateOperation")]
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
                Pageable<OperationModelTableEntity> entities = client.Query<OperationModelTableEntity>();

                OperationModelTableEntity requestTableEntity = new OperationModelTableEntity();

                requestTableEntity.RowKey = RequestObject.RowKey;
                requestTableEntity.PartitionKey = RequestObject.PartitionKey;
                requestTableEntity.Title = RequestObject.Title;
                requestTableEntity.Description = RequestObject.Description;
                requestTableEntity.StartDate = DateTime.SpecifyKind(Convert.ToDateTime(RequestObject.StartDate), DateTimeKind.Utc);
                requestTableEntity.TargetDate = DateTime.SpecifyKind(Convert.ToDateTime(RequestObject.TargetDate), DateTimeKind.Utc);
                requestTableEntity.Icon = RequestObject.Icon;

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
