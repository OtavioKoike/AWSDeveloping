using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace DynamoDBOperations
{
    class CreateTableTask
    {
        class TableDefinition
        {
            public string TableName { get; set; }
            public string PartitionKey { get; set; }
            public string SortKey { get; set; }
            public long ReadCapacity { get; set; }
            public long WriteCapacity { get; set; }
        }

        public async Task Run()
        {
            var configSettings = ConfigSettingsReader<DynamoDBConfigSettings>.Read("DynamoDB");

            try
            {
                IAmazonDynamoDB ddbClient = null;

                // TODO 1: create an Amazon DynamoDB service client to pass to the
                // main function.
                ddbClient = new AmazonDynamoDBClient();

                // End TODO1

                var tableDefinition = new TableDefinition
                {
                    TableName = configSettings.TableName,
                    PartitionKey = configSettings.PartitionKey,
                    SortKey = configSettings.SortKey,
                    ReadCapacity = configSettings.ReadCapacity,
                    WriteCapacity = configSettings.WriteCapacity
                };

                Console.WriteLine($"\nCreating an Amazon DynamoDB table \"{configSettings.TableName}\"");
                Console.WriteLine($"with a partition key: {configSettings.PartitionKey}");
                Console.WriteLine($"and sort key: {configSettings.SortKey}.\n");

                var createResponse = await CreateTable(ddbClient, tableDefinition);
                Console.WriteLine($"Table Status: {createResponse.TableDescription.TableStatus}");

                Console.WriteLine("\nWaiting for the table to be available...");
                await WaitForTableCreation(ddbClient, configSettings.TableName);

                Console.WriteLine("\nTable is now available.");
                var tableOutput = await GetTableInfo(ddbClient, configSettings.TableName);
                Console.WriteLine($"Table Status: {tableOutput.Table.TableStatus}");

                // Resultado no final do Arquivo*
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }
        }

        async Task<CreateTableResponse> CreateTable(IAmazonDynamoDB ddbClient, TableDefinition tableDefinition)
        {
            CreateTableResponse response = null;

            // TODO 2: Add logic to create a table with UserId as the 
            // partition key and NoteId as the sort key, and return the
            // table's status.
            var request = new CreateTableRequest
            {
                TableName = tableDefinition.TableName,
                ProvisionedThroughput = new ProvisionedThroughput
                {
                    ReadCapacityUnits = tableDefinition.ReadCapacity,
                    WriteCapacityUnits = tableDefinition.WriteCapacity
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement
                    {
                        AttributeName = tableDefinition.PartitionKey,
                        KeyType = KeyType.HASH
                    },
                    new KeySchemaElement
                    {
                        AttributeName = tableDefinition.SortKey,
                        KeyType = KeyType.RANGE
                    }
                },
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition
                    {
                        AttributeName = tableDefinition.PartitionKey,
                        AttributeType = "S"
                    },
                    new AttributeDefinition
                    {
                        AttributeName = tableDefinition.SortKey,
                        AttributeType = "N"
                    }
                }
            };

            response = await ddbClient.CreateTableAsync(request);

            // End TODO 2

            return response;
        }

        async Task WaitForTableCreation(IAmazonDynamoDB ddbClient, string tableName)
        {
            // TODO 3: wait for creation of your new table to complete
            var response = await ddbClient.DescribeTableAsync(tableName);
            while (response.Table.TableStatus != TableStatus.ACTIVE)
            {
                Thread.Sleep(500);
                response = await ddbClient.DescribeTableAsync(tableName);
            }

            // Note: The AWS SDK for .NET does not contain support for 'waiters',
            // that can be found in other AWS SDKs. Therefore we need to loop,
            // testing the table status, with a short sleep period between tests.

            // End TODO 3
        }

        async Task<DescribeTableResponse> GetTableInfo(IAmazonDynamoDB ddbClient, string tableName)
        {
            return await ddbClient.DescribeTableAsync(tableName);
        }
    }
}

// Creating an Amazon DynamoDB table "Notes"
// with a partition key: UserId
// and sort key: NoteId.

// Table Status: CREATING

// Waiting for the table to be available...

// Table is now available.
// Table Status: ACTIVE

// Press any key to exit...