using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;

namespace DynamoDBOperations
{
    class LoadDataTask
    {
        public async Task Run()
        {
            var configSettings = ConfigSettingsReader<DynamoDBConfigSettings>.Read("DynamoDB");

            try
            {
                var ddbClient = new AmazonDynamoDBClient();

                var tableName = configSettings.TableName;
                var jsonFilename = configSettings.Sourcenotes;

                var pathedJsonFilename = Path.Join(Environment.CurrentDirectory, jsonFilename);

                var jsonString = await File.ReadAllTextAsync(pathedJsonFilename);
                var json = JsonDocument.Parse(jsonString);

                var table = Table.LoadTable(ddbClient, tableName);

                Console.WriteLine($"\nLoading {tableName} table with data from file {jsonFilename}\n");

                var root = json.RootElement;
                foreach (var note in root.EnumerateArray())
                {
                    await PutNote(table, note);
                }

                Console.WriteLine("\nFinished loading notes from the JSON file.");
                
                // Resultado no final do Arquivo*
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }
        }

        async Task PutNote(Table table, JsonElement note)
        {
            Console.WriteLine($"Loading note {note}");

            // TODO 4: Add code that uses the function parameters to 
            // add a new note to the table.
            var document = new Document
            {
                ["UserId"] = note.GetProperty("UserId").GetString(),
                ["NoteId"] = int.Parse(note.GetProperty("NoteId").GetString()),
                ["Note"] = note.GetProperty("Note").GetString()
            };

            await table.PutItemAsync(document);


            // End TODO 4
        }
    }
}

// Loading Notes table with data from file notes.json

// Loading note {"UserId": "testuser","NoteId": "001", "Note": "hello"}
// Loading note {"UserId": "testuser","NoteId": "002", "Note": "this is my first note"}
// Loading note {"UserId": "newbie","NoteId": "001", "Note": "Free swag code: 1234"}
// Loading note {"UserId": "newbie","NoteId": "002", "Note": "I love DynamoDB"}
// Loading note {"UserId": "student","NoteId": "001", "Note": "DynamoDB is NoSQL"}
// Loading note {"UserId": "student","NoteId": "002", "Note": "A DynamoDB table is schemaless"}
// Loading note {"UserId": "student","NoteId": "003", "Note": "PartiQL is a SQL compatible language for DynamoDB"}
// Loading note {"UserId": "student","NoteId": "005", "Note": "Maximum size of an item is ____ KB ?"}
// Loading note {"UserId": "student","NoteId": "004", "Note": "I love DyDB"}

// Finished loading notes from the JSON file.

// Press any key to exit...