// PollyNotes-CreateUpdateFunction
// This function allows us to create and update items in DynamoDB
//
// This lambda function is integrated with the following API method:
// /notes POST (create or update a note)

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

// Assembly attribute to enable the JSON input to Lambda functions to be converted into a .NET class.
[assembly:LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace PollyNotesAPICreateUpdate
{
    public class EventData
    {
        public string UserId { get; set; }
        public string NoteId { get; set; }
        public string Note { get; set; }
    }

    public class Function
    {
        public async Task<string> FunctionHandler(EventData evt, ILambdaContext context)
        {
            var tableName = Environment.GetEnvironmentVariable("TABLE_NAME");

            var ddbClient = new AmazonDynamoDBClient();

            var itemAttributes = new Dictionary<string, AttributeValue>
            {
                { "UserId", new AttributeValue(evt.UserId) },
                { "NoteId", new AttributeValue { N = evt.NoteId } },
                { "Note", new AttributeValue(evt.Note) }
            };

            await ddbClient.PutItemAsync(tableName, itemAttributes);

            return evt.NoteId;
        }
    }
}
