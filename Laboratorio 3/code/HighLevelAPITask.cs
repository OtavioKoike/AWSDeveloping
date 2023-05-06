using System;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

namespace DynamoDBOperations
{
    // This class represents a data item (row) in the database. A class
    // attribute maps it to the underlying table, and we can optionally
    // attribute the class properties as shown.
    [DynamoDBTable("Notes")]
    class Note
    {
        // This attribute can be used to map the hash (primary) key for the table.
        // Because the class property in this example has the same name as the
        // underlying table attribute, we could remove the property attribution
        // and the SDK will infer the mapping based on matching property name.
        //
        // In this lab we are using primitive string and int types in the database.
        // You can also map aribitrary data types if you provide an appropriate
        // converter.
        [DynamoDBHashKey]
        public string UserId { get; set; }

        // This attribute can be used to map the range (sort) key for the table.
        // Like the primary key above, because the property name matches the
        // data attribute name in the underlying table, we could remove the
        // attribution and the SDK will infer the mapping based on matching name.
        [DynamoDBRangeKey]
        public int NoteId { get; set; }

        // C# class members may not have the same name as their enclosing class. This
        // attribute maps the renamed class property to the underlying attribute in
        // the table.
        [DynamoDBProperty("Note")]
        public string NoteText { get; set; }

        // This is an example showing that you do not need to map every property in the
        // class to the attributes in the table.
        [DynamoDBIgnore]
        public string SomeOtherData { get; set; }

        // simple helper used to pretty-print the object
        public string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }
    }

    class HighLevelAPITask
    {
        public async Task Run()
        {
            var configSettings = ConfigSettingsReader<DynamoDBConfigSettings>.Read("DynamoDB");

            try
            {
                var ddbClient = new AmazonDynamoDBClient();

                // the context object provides the connection and mapping to the
                // underlying database in the object persistence model
                var ddbContext = new DynamoDBContext(ddbClient);

                var tableName = configSettings.TableName;
                var userId = configSettings.QueryUserId;
                var noteId = configSettings.QueryNoteId;

                Console.WriteLine($"\n************\nUsing high-level object persistence model to query for note {noteId} that belongs to user {userId}...\n");

                var note = await QuerySpecificNote(ddbContext, userId, noteId);

                // update the Note object and store it back in the database
                var originalNoteText = note.NoteText;
                note.NoteText += ", updated using the object persistence model!";

                Console.WriteLine($"\nUpdating the text of the queried note...");
                await UpdateNote(ddbContext, note);

                Console.WriteLine($"\nVerifying update by requerying note...");
                var updatedNote = await QuerySpecificNote(ddbContext, userId, noteId);

                // restore the original object's note text, for consistency in later labs!
                Console.WriteLine($"\nRestoring original note text...");
                note.NoteText = originalNoteText;
                await UpdateNote(ddbContext, note);

                Console.WriteLine("\nData query and update using high-level object persistence model completed successfully.");
                
                // Resultado no final do Arquivo*
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }
        }

        async Task<Note> QuerySpecificNote(DynamoDBContext ddbContext, string userId, int noteId)
        {
            var note = await ddbContext.LoadAsync<Note>(userId, noteId);

            Console.WriteLine($"Returned note object: {note.ToJson()}");

            return note;
        }

        async Task UpdateNote(DynamoDBContext ddbContext, Note note)
        {
            await ddbContext.SaveAsync(note);

            Console.WriteLine("Note updated!");
        }
    }
}

// Using high-level object persistence model to query for note 5 that belongs to user student...

// Returned note object: {"UserId":"student","NoteId":5,"NoteText":"maximum item size in DynamoDB is 400 KB","SomeOtherData":null}

// Updating the text of the queried note...
// Note updated!

// Verifying update by re-querying note...
// Returned note object: {"UserId":"student","NoteId":5,"NoteText":"maximum item size in DynamoDB is 400 KB, updated using the object persistence model!","SomeOtherData":null}

// Restoring original note text...
// Note updated!

// Data query and update using high-level object persistence model completed successfully.

// Press any key to exit...