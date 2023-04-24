using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using System.Text;
using Newtonsoft.Json;
using Amazon.S3;
using Amazon.S3.Transfer;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace LibrarySqsLambda
{
    public class Function
    {
        private const string _bucketName = "lambda-code-bucket-new";
        /// <summary>
        /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
        /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
        /// region the Lambda function is executed in.
        /// </summary>
        public Function()
        {

        }


        /// <summary>
        /// This method is called for every Lambda invocation. This method takes in an SQS event object and can be used 
        /// to respond to SQS messages.
        /// </summary>
        /// <param name="evnt"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
        {
            context.Logger.LogInformation($"Received {evnt.Records.Count} messages");

            var s3Client = new AmazonS3Client();
            var transferUtility = new TransferUtility(s3Client);

            foreach (var message in evnt.Records)
            {
                // create JSON object with the message body
                var jsonObject = new { Body = message.Body };

                // serialize JSON object to string
                var jsonString = JsonConvert.SerializeObject(jsonObject);

                // upload JSON string to S3
                var key = $"messages/{Guid.NewGuid()}.json";
                var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
                await transferUtility.UploadAsync(memoryStream, _bucketName, key);

                context.Logger.LogInformation($"Uploaded to S3 bucket: {key}");
            }
        }

        private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
        {
            context.Logger.LogInformation($"Processed message {message.Body}");

            // TODO: Do interesting work based on the new message
            await Task.CompletedTask;
        }
    }
}