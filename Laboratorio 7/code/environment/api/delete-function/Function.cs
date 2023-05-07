// PollyNotes-DeleteFunction
// This function allows us to delete items in DynamoDB
//
// This Lambda function is integrated with the following API method:
// /notes/{id} DELETE (delete a note)

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;

//#### TODO 1: Import required namespaces for X-Ray
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;

//#### End TODO 1

// Assembly attribute to enable the JSON input to Lambda functions to be converted into a .NET class.
[assembly:LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace PollyNotesAPIDelete
{
    public class Function
    {
        private void AddAnnotation(string userId, string noteId)
        {
            //#### TODO 2: add UserId and NoteId as annotations
            AWSXRayRecorder.Instance.BeginSubsegment("Delete a note");
            AWSXRayRecorder.Instance.AddAnnotation("UserId", userId);
            AWSXRayRecorder.Instance.AddAnnotation("NoteId", noteId);
            AWSXRayRecorder.Instance.EndSubsegment();

            //#### End TODO 2
        }

        public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
        {
            Console.WriteLine("Initiating PollyNotes-DeleteFunction");

            //#### TODO 3: Register calls to DynamoDB, from the AWS SDK for .NET, for X-Ray tracing
            AWSSDKHandler.RegisterXRay<IAmazonDynamoDB>();

            //#### End TODO 3

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
                var noteId = apigProxyEvent.PathParameters["id"];
                var tableName = Environment.GetEnvironmentVariable("TABLE_NAME");

                AddAnnotation(userId, noteId);

                var ddbClient = new AmazonDynamoDBClient();
                var table = Table.LoadTable(ddbClient, tableName);

                await table.DeleteItemAsync(userId, Int32.Parse(noteId));

                apiResponse.Body = noteId;
                apiResponse.StatusCode = 200;
            }
            catch (AmazonDynamoDBException e)
            {
                context.Logger.Log($"DynamoDB exception during deletion: {e.Message}");
                apiResponse.Body = e.Message;
            }
            catch (Exception e)
            {
                context.Logger.Log($"Exception during deletion: {e.Message}");
                apiResponse.Body = JsonSerializer.Serialize(e);
            }

            return apiResponse;
        }
    }
}
