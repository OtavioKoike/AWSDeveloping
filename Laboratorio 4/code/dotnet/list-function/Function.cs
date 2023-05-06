// PollyNotes-ListFunction
//
// This lambda function is integrated with the following API methods:
// /notes GET (list operation)
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

namespace PollyNotesAPIList
{
    public class EventData
    {
        public string UserId { get; set; }
    }

    public class Function
    {
        public async Task<List<object>> FunctionHandler(EventData evt, ILambdaContext context)
        {
            var tableName = Environment.GetEnvironmentVariable("TABLE_NAME");

            var ddbClient = new AmazonDynamoDBClient();
            var table = Table.LoadTable(ddbClient, tableName);

            // if a user was passed, query the table for that user's items
            Search search;
            if (!string.IsNullOrEmpty(evt.UserId))
            {
                search = table.Query(evt.UserId, new QueryFilter());
            }
            else
            {
                search = table.Scan(new ScanFilter());
            }

            var output = new List<object>();

            // DynamoDB returns the query results in batches so loop, and
            // translate as required into the desired output format.
            do
            {
                var resultBatch = await search.GetNextSetAsync();
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
            } while (!search.IsDone);

            return output;
        }
    }
}
