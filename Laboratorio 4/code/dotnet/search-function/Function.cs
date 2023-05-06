// PollyNotes-SearchFunction
//
// This lambda function is integrated with the following API methods:
// /notes/search GET (search)
//
// Its purpose is to get notes from our DynamoDB table

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;

// Assembly attribute to enable the JSON input to Lambda functions to be converted into a .NET class.
[assembly:LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace PollyNotesAPISearch
{
    public class EventData
    {
        public string UserId { get; set; }
        public string Text { get; set; }
    }

    public class Function
    {
        public async Task<List<object>> FunctionHandler(EventData evt, ILambdaContext context)
        {
            var tableName = Environment.GetEnvironmentVariable("TABLE_NAME");

            var ddbClient = new AmazonDynamoDBClient();
            var table = Table.LoadTable(ddbClient, tableName);

            // Construct the query to be run (this does not return results at this
            // stage)
            var query = table.Query(evt.UserId, new QueryFilter());
            if (!(string.IsNullOrEmpty(evt.Text)))
            {
                // Add a filter expression for the supplied query text
                var expression = new Expression();
                expression.ExpressionStatement = "contains(Note, :queryText)";
                expression.ExpressionAttributeValues[":queryText"] = evt.Text;
                query.FilterExpression = expression;
            }

            var output = new List<object>();

            // DynamoDB returns the query results in batches so loop, and
            // translate as required into the desired output format.
            do
            {
                var resultBatch = await query.GetNextSetAsync();
                // each item in the batch is a Document instance containing
                // a collection of attributes, mapped to the column names. The
                // value for each attribute is enclosed in a DynamoDBEntry type.
                foreach (var item in resultBatch)
                {
                    output.Add(new
                    {
                        userId = item["UserId"].AsString(),
                        noteId = item["NoteId"].AsString(),
                        note = item["Note"].AsString()
                    });
                }
            } while (!query.IsDone);

            return output;
        }
    }
}
