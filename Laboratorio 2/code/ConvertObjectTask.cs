using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;

namespace S3Operations
{
    class ConvertObjectTask
    {
        public async Task Run()
        {
            Console.WriteLine("\nStart of convert object task");

            Console.WriteLine("\nReading configuration for bucket name...");
            var configSettings = ConfigSettingsReader<S3ConfigSettings>.Read("S3");

            try
            {
                using var s3Client = new AmazonS3Client();

                // Get the object from Amazon S3
                Console.WriteLine("\nGetting the CSV object from the Amazon S3 bucket");
                var csvData = await GetCSVFile(s3Client,
                                               configSettings.BucketName,
                                               $"{configSettings.ObjectName}{configSettings.SourceFileExtension}");

                // Convert the object to the new format
                Console.WriteLine("\nConverting CSV string to JSON...");
                var jsonData = ConvertToJSON(csvData);

                // Uploaded the converted object to Amazon S3
                Console.WriteLine("\nCreating the new JSON object in Amazon S3...");
                await CreateObject(s3Client,
                                   configSettings.BucketName,
                                   $"{configSettings.ObjectName}{configSettings.ProcessedFileExtension}",
                                   jsonData,
                                   configSettings.ProcessedContentType,
                                   new Dictionary<string, string>
                                   {
                                       { configSettings.MetadataKey, configSettings.MetadataValue }
                                   });
                
                // Resultado no final do Arquivo*
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }

            Console.WriteLine("\nEnd of convert object task");
        }

        async Task<string> GetCSVFile(IAmazonS3 s3Client, string bucketName, string name)
        {
            string contents = null;

            // Start TODO 6: Download the file contents to the
            // contents object so that it can be decoded to a string.
            var request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = name
            };

            var response = await s3Client.GetObjectAsync(request);
            using (var reader = new StreamReader(response.ResponseStream))
            {
                contents = reader.ReadToEnd();
            }

            // End TODO 6

            return contents;
        }

        async Task CreateObject(IAmazonS3 s3Client,
                                string bucketName,
                                string name,
                                string data,
                                string contentType,
                                IDictionary<string, string> metadata)
        {
            var request = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = name,
                ContentBody = data,
                ContentType = contentType
            };

            foreach (var key in metadata.Keys)
            {
                request.Metadata.Add(key, metadata[key]);
            }
            // Start TODO 7: Create an Amazon S3 object with the converted data
            await s3Client.PutObjectAsync(request);

            // End TODO 7

            Console.WriteLine("Successfully created object");
        }

        string ConvertToJSON(string csv)
        {
            // simple conversion, appropriate to the lab data

            var rows = csv.Split('\n');
            var headers = rows[0].Split(',');

            var data = new List<Dictionary<string, string>>();

            for (var row = 1; row < rows.Length; row++)
            {
                var dict = new Dictionary<string, string>();

                var rowData = rows[row].Split(',');

                var col = 0;
                foreach (var header in headers)
                {
                    dict.Add(header, rowData[col++]);
                }

                data.Add(dict);
            }

            var jsonData = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            return jsonData;
        }
    }
}

// Start of convert object task

// Reading configuration for bucket name...

// Getting the CSV object from Amazon S3 bucket

// Converting CSV string to JSON...

// Creating the new JSON object in Amazon S3...
// Successfully created object

// End of convert object task

// Press any key to exit...