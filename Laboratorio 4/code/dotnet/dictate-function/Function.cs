// PollyNotes-DictateFunction
//
// This lambda function uses Polly to convert a note to speech, uploads the mp3 file to S3,
// and returns a signed URL.
//
// This lambda function is integrated with the following API methods:
// /notes/{id}/POST

using System;
using System.Threading.Tasks;
using System.IO;
using Amazon.Lambda.Core;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Polly;
using Amazon.Polly.Model;

// Assembly attribute to enable the JSON input to Lambda functions to be converted into a .NET class.
[assembly:LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace PollyNotesAPIDictate
{
    public class EventData
    {
        public string UserId { get; set; }
        public string NoteId { get; set; }
        public string VoiceId { get; set;}
    }

    public class Function
    {
        public async Task<string> FunctionHandler(EventData evt, ILambdaContext context)
        {
            var tableName = Environment.GetEnvironmentVariable("TABLE_NAME");
            var mp3Bucket = Environment.GetEnvironmentVariable("MP3_BUCKET_NAME");

            // Get the note text from the database
            var text = await GetNote(tableName, evt.UserId, evt.NoteId);

            // Save a MP3 file locally with the output from Polly
            var filePath = await CreateMP3File(evt.NoteId, text, evt.VoiceId);

            // Host the file on S3 that is accessed by a pre-signed url
            var signedUrl = await HostFileOnS3(filePath, mp3Bucket, evt.UserId, evt.NoteId);

            return signedUrl;
        }

        public async Task<string> GetNote(string tableName, string userId, string noteId)
        {
            // TODO 1: Get the note text from the notes DynamoDB table that matches the userId and noteId
            var ddbClient = new AmazonDynamoDBClient();
            
            var table = Table.LoadTable(ddbClient, tableName);
            var item = await table.GetItemAsync(userId, Int32.Parse(noteId));
            
            return item["Note"];

            // End TODO 1
        }

        public async Task<string> CreateMP3File(string noteId, string text, string voiceId)
        {		
            var pollyClient = new AmazonPollyClient();
            // TODO 2: Use Polly to convert the note text to speech using the voiceId
            // and save the file as an MP3 in the /tmp folder
            var request = new SynthesizeSpeechRequest
            {
                OutputFormat = OutputFormat.Mp3,
                Text = text,
                VoiceId = voiceId
            };

            // End TODO 2
            var response = await pollyClient.SynthesizeSpeechAsync(request);

            // Save the audio stream returned by Amazon Polly on Lambda's temp
            // directory '/tmp'
            var tmpFilename = $"/tmp/{noteId}";
			if (response.AudioStream != null)
			{
				using (var fs = File.Create(tmpFilename))
				{
					await response.AudioStream.CopyToAsync(fs);
				}
			}
			
            return tmpFilename;
			
        }

        public async Task<string> HostFileOnS3(string filePath, string mp3Bucket, string userId, string noteId)
        {			
            var objectKey = $"{userId}/{noteId}.mp3";
            var s3Client = new AmazonS3Client();

            var putRequest = new PutObjectRequest
            {
                BucketName = mp3Bucket,
                Key = objectKey,
                FilePath = filePath
            };
            await s3Client.PutObjectAsync(putRequest);

            // Generate a pre-signed URL
            var urlRequest = new GetPreSignedUrlRequest
            {
                BucketName = mp3Bucket,
                Key = objectKey,
                Verb = HttpVerb.GET,
                Expires = DateTime.Now.AddDays(1)
            };

            // TODO 3: Upload the mp3 file to S3 mp3Bucket and generate a pre-signed URL to access the MP3 object
            return s3Client.GetPreSignedURL(urlRequest);

            // End TODO 3
        }
    }
}
