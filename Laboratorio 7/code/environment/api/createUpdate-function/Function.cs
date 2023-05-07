// PollyNotes-CreateUpdateFunction
// This function allows us to create and update items in DynamoDB
//
// This Lambda function is integrated with the following API method:
// /notes POST (create or update a note)

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;

// Assembly attribute to enable the JSON input to Lambda functions to be converted into a .NET class.
[assembly:LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace PollyNotesAPICreateUpdate
{
    public class Function
    {
        private void AddAnnotation(string userId, string noteId)
        {
            AWSXRayRecorder.Instance.BeginSubsegment("Put a note");
            AWSXRayRecorder.Instance.AddAnnotation("UserId", userId);
            AWSXRayRecorder.Instance.AddAnnotation("NoteId", noteId);
            AWSXRayRecorder.Instance.EndSubsegment();
        }

        public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
        {
            Console.WriteLine("Initiating PollyNotes-CreateUpdateFunction");

            // Register calls to DynamoDB, from the AWS SDK for .NET, for X-Ray tracing
            AWSSDKHandler.RegisterXRay<IAmazonDynamoDB>();

            // create the response object, the error code is 500 unless manually set to a success
            var apiResponse = new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" },
                    { "Access-Control-Allow-Origin", "*" }
                }
            };

            try
            {
                var userId = apigProxyEvent.RequestContext.Authorizer.Claims["cognito:username"];
                var tableName = Environment.GetEnvironmentVariable("TABLE_NAME");

                Console.WriteLine($"Event body = {apigProxyEvent.Body}");

                var noteData = JsonDocument.Parse(apigProxyEvent.Body).RootElement;

                var noteId = noteData.GetProperty("NoteId").ToString();
                var note = noteData.GetProperty("Note").GetString();

                // Add X-Ray annotations to the trace
                AddAnnotation(userId, noteId);

                var ddbClient = new AmazonDynamoDBClient();
                var table = Table.LoadTable(ddbClient, tableName);

                var document = new Document();
                document["UserId"] = userId;
                document["NoteId"] = Int32.Parse(noteId);
                document["Note"] = note;

                await table.PutItemAsync(document);

                apiResponse.Body = $"{noteId}";
                apiResponse.StatusCode = 200;
            }
            catch (AmazonDynamoDBException e)
            {
                context.Logger.Log($"DynamoDB exception creating/updating note: {e.Message}");
                apiResponse.Body = e.Message;
            }
            catch (NotSupportedException e)
            {
                // this will come from json parse if you passed a quoted number in the body
                context.Logger.Log($"Not supported exception creating/updating note: {e.Message}");
                apiResponse.Body = e.Message;
            }
            catch (Exception e)
            {
                context.Logger.Log($"Exception during create/update: {e.Message}");
                apiResponse.Body = JsonSerializer.Serialize(e);
            }

            return apiResponse;
        }
    }
}
