using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace DynamoDBOperations
{
    class UpdateDataTask
    {
        public async Task Run()
        {
            var configSettings = ConfigSettingsReader<DynamoDBConfigSettings>.Read("DynamoDB");

            try
            {
                var ddbClient = new AmazonDynamoDBClient();

                var tableName = configSettings.TableName;
                var userId = configSettings.QueryUserId;
                var noteId = configSettings.QueryNoteId;
                var notePrefix = configSettings.NotePrefix;

                Console.WriteLine("\nUpdating the note flag for remediation...\n");

                await UpdateNewAttribute(ddbClient, tableName, userId, noteId);

                Console.WriteLine("\nRemediating the marked note...\n");
                await UpdateExistingAttributeConditionally(ddbClient, tableName, userId, noteId, notePrefix);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }
        }

        async Task UpdateNewAttribute(IAmazonDynamoDB ddbClient, string tableName, string userId, int noteId)
        {
            Dictionary<string, AttributeValue> attributes = null;

            // TODO 7: Add code to set an 'Is_Incomplete' flag to 'Yes' for the note that matches the 
            // provided function parameters
            var request = new UpdateItemRequest
            {
                TableName = tableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "UserId", new AttributeValue(userId) },
                    { "NoteId", new AttributeValue { N = noteId.ToString() } }
                },
                            ReturnValues = ReturnValue.ALL_NEW,
                            UpdateExpression = "SET Is_Incomplete = :incomplete",
                            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":incomplete", new AttributeValue("Yes") }
                }
            };

            var response = await ddbClient.UpdateItemAsync(request);
            attributes = response.Attributes;

            // End TODO 7

            Print(attributes);
        }

        async Task UpdateExistingAttributeConditionally(IAmazonDynamoDB ddbClient, string tableName, string userId, int noteId, string notePrefix)
        {
            Dictionary<string, AttributeValue> attributes = null;

            try
            {
                // TODO 8: Update existing maxSize value to correct value
                var maxSize = "400 KB";

                // End TODO 8
                
                var request = new UpdateItemRequest
                {
                    TableName = tableName,
                    Key = new Dictionary<string, AttributeValue>
                    {
                        { "UserId", new AttributeValue(userId) },
                        { "NoteId", new AttributeValue { N = noteId.ToString() } }
                    },
                    ReturnValues = ReturnValue.ALL_NEW,
                    UpdateExpression = "SET Note = :NewNote, Is_Incomplete = :new_incomplete",
                    ConditionExpression = "Is_Incomplete = :old_incomplete",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        { ":NewNote", new AttributeValue($"{notePrefix} {maxSize}") },
                        { ":new_incomplete", new AttributeValue("No") },
                        { ":old_incomplete", new AttributeValue("Yes") }
                    }
                };

                var response = await ddbClient.UpdateItemAsync(request);
                attributes = response.Attributes;

                Print(attributes);
            }
            catch (ConditionalCheckFailedException)
            {
                Console.WriteLine("Sorry, your update is invalid mate!");
            }
        }

        void Print(IDictionary<string, AttributeValue> attributes)
        {
            var json = JsonSerializer.Serialize(new
            {
                UserId = new
                {
                    S = attributes["UserId"].S
                },
                NoteId = new
                {
                    N = attributes["NoteId"].N.ToString()
                },
                Note = new
                {
                    S = attributes["Note"].S
                },
                Is_Incomplete = new
                {
                    S = attributes["Is_Incomplete"].S
                }
            });

            Console.WriteLine($"{json}");
        }
    }
}

// Updating the note flag for remediation...

// {"UserId":{"S":"student"},"NoteId":{"N":"5"},"Note":{"S":"Maximum size of an item is ____ KB ?"},"Is_Incomplete":{"S":"Yes"}}

// Remediating the marked note...

// {"UserId":{"S":"student"},"NoteId":{"N":"5"},"Note":{"S":"maximum item size in DynamoDB is 400 KB"},"Is_Incomplete":{"S":"No"}}

// Press any key to exit...