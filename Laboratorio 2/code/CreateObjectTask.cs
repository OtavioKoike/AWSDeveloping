using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;

namespace S3Operations
{
    class CreateObjectTask
    {
        public async Task Run()
        {
            Console.WriteLine("\nStart of create object task");

            Console.WriteLine("\nReading configuration for bucket name...");
            var configSettings = ConfigSettingsReader<S3ConfigSettings>.Read("S3");

            try
            {
                using var s3Client = new AmazonS3Client();

                // Create object in the Amazon S3 bucket
                await UploadObject(s3Client,
                                   configSettings.BucketName,
                                   $"{configSettings.ObjectName}{configSettings.SourceFileExtension}",
                                   configSettings.SourceContentType,
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

            Console.WriteLine("\nEnd of create object task");
        }

        async Task UploadObject(IAmazonS3 s3Client,
                                string bucketName,
                                string name,
                                string contentType,
                                IDictionary<string, string> metadata)
        {
            Console.WriteLine("Creating object...");

            // Start TODO 5: create a object by transferring the file to the S3 bucket,
            // set the contentType of the file and add any metadata passed to this function.
            var request = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = name,
                FilePath = System.IO.Path.Join(Environment.CurrentDirectory, name),
                ContentType = contentType
            };

            foreach (var key in metadata.Keys)
            {
                request.Metadata.Add(key, metadata[key]);
            }

            await s3Client.PutObjectAsync(request);

            // End TODO 5

            Console.WriteLine("Finished creating object");
        }
    }
}

// Start of create object task

// Reading configuration for bucket name...
// Creating object...
// Finished creating object

// End of create object task

// Press any key to exit...