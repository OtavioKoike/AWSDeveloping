// PollyNotes-SearchFunction
//
// This lambda function is integrated with the following API methods:
// /notes/search GET (search)
//
// Its purpose is to get notes from our DynamoDB table

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

namespace PollyNotesAPISearch
{
    public class Function
    {
        private void AddAnnotation(string userId, string queryText)
        {
            AWSXRayRecorder.Instance.BeginSubsegment("Search notes");
            AWSXRayRecorder.Instance.AddAnnotation("UserId", userId);
            AWSXRayRecorder.Instance.AddAnnotation("query", queryText);
            AWSXRayRecorder.Instance.EndSubsegment();
        }

        public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
        {
            Console.WriteLine("Initiating PollyNotes-SearchFunction");

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
                apigProxyEvent.QueryStringParameters.TryGetValue("text", out string queryText);

                // Add X-Ray annotations to the trace
                AddAnnotation(userId, queryText ?? "");

                var ddbClient = new AmazonDynamoDBClient();
                var table = Table.LoadTable(ddbClient, tableName);

                // Construct the query to be run (this does not return results at this
                // stage)
                var query = table.Query(userId, new QueryFilter());
                if (!(string.IsNullOrEmpty(queryText)))
                {
                    // Add a filter expression for the supplied query text
                    var expression = new Expression();
                    expression.ExpressionStatement = "contains(Note, :queryText)";
                    expression.ExpressionAttributeValues[":queryText"] = queryText;
                    query.FilterExpression = expression;
                }


                var output = new List<object>();

                // DynamoDB returns the query results in batches so loop, and
                // translate as required into the desired output format.
                do
                {
                    var resultBatch = await query.GetNextSetAsync();
                    // each item in the batch is a Document instance containing
                    // a collection of keys, mapped to the column names. The
                    // value for each key is enclosed in a DynamoDBEntry type.
                    foreach (var item in resultBatch)
                    {
                        output.Add(new
                        {
                            UserId = item["UserId"].AsString(),
                            NoteId = item["NoteId"].AsInt(),
                            Note = item["Note"].AsString()
                        });
                    }
                } while (!query.IsDone);

                apiResponse.Body =JsonSerializer.Serialize(output);
                apiResponse.StatusCode = 200;
            }
            catch (AmazonDynamoDBException e)
            {
                context.Logger.Log($"DynamoDB exception during query: {e.Message}");
                apiResponse.Body = e.Message;
            }
            catch (ArgumentException e)
            {
                context.Logger.Log($"Argument exception: {e.Message}");
                apiResponse.Body = e.Message;
            }
            catch (Exception e)
            {
                context.Logger.Log($"Exception during query: {e.Message}");
                apiResponse.Body = JsonSerializer.Serialize(e);
            }

            return apiResponse;
        }
    }
}
