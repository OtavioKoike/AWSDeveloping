using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace DynamoDBOperations
{
    class PaginateDataTask
    {
        public async Task Run()
        {
            var configSettings = ConfigSettingsReader<DynamoDBConfigSettings>.Read("DynamoDB");

            try
            {
                var ddbClient = new AmazonDynamoDBClient();

                var tableName = configSettings.TableName;
                var pageSize = configSettings.PageSize;

                Console.WriteLine("\nScanning with pagination...\n");

                await QueryAllNotesPaginator(ddbClient, tableName, pageSize);
                
                // Resultado no final do Arquivo*
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }
        }

        async Task QueryAllNotesPaginator(IAmazonDynamoDB ddbClient, string tableName, int pageSize)
        {
            // TODO 6: Add code that creates a paginator and prints the returned items
            var request = new ScanRequest
            {
                TableName = tableName,
                Limit = pageSize,
                ProjectionExpression = "UserId, NoteId, Note"
            };
            var paginator = ddbClient.Paginators.Scan(request);

            var pageNumber = 1;
            await foreach (var page in paginator.Responses)
            {
                if (page.Items.Count > 0)
                {
                    Console.WriteLine($"Starting page {pageNumber}");
                    Print(page.Items);
                    Console.WriteLine($"End of page {pageNumber++}\n");
                }
            }

            // End TODO 6
        }

        void Print(IEnumerable<Dictionary<string, AttributeValue>> notes)
        {
            foreach (var note in notes)
            {
                var json = JsonSerializer.Serialize(new
                {
                    UserId = note["UserId"].S,
                    NoteId = note["NoteId"].N.ToString(),
                    Note = note["Note"].S
                });

                Console.WriteLine(json.ToString());
            }
        }
    }
}

// Scanning with pagination...

// Starting page 1
// {"UserId":"testuser","NoteId":"1","Note":"hello"}
// {"UserId":"testuser","NoteId":"2","Note":"this is my first note"}
// {"UserId":"student","NoteId":"1","Note":"DynamoDB is NoSQL"}
// End of page 1

// Starting page 2
// {"UserId":"student","NoteId":"2","Note":"A DynamoDB table is schemaless"}
// {"UserId":"student","NoteId":"3","Note":"PartiQL is a SQL compatible language for DynamoDB"}
// {"UserId":"student","NoteId":"4","Note":"I love DyDB"}
// End of page 2

// Starting page 3
// {"UserId":"student","NoteId":"5","Note":"Maximum size of an item is ____ KB ?"}
// {"UserId":"newbie","NoteId":"1","Note":"Free swag code: 1234"}
// {"UserId":"newbie","NoteId":"2","Note":"I love DynamoDB"}
// End of page 3


// Press any key to exit...