// PollyNotes-DictateFunction
//
// This lambda function uses Polly to convert a note to speech, uploads the mp3 file to S3,
// and returns a signed URL.
//
// This lambda function is integrated with the following API methods:
// /notes/{id}/POST

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Polly;
using Amazon.Polly.Model;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;

// Assembly attribute to enable the JSON input to Lambda functions to be converted into a .NET class.
[assembly:LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace PollyNotesAPIDictate
{
    public class Function
    {
        private void AddAnnotation(string userId, string noteId, string voiceId)
        {
            AWSXRayRecorder.Instance.BeginSubsegment("Dictate a note");
            AWSXRayRecorder.Instance.AddAnnotation("UserId", userId);
            AWSXRayRecorder.Instance.AddAnnotation("NoteId", noteId);
            AWSXRayRecorder.Instance.AddAnnotation("VoiceId", voiceId);
            AWSXRayRecorder.Instance.EndSubsegment();
        }

        public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
        {
            Console.WriteLine("Initiating PollyNotes-DictateFunction");

            // Register calls made to AWS services, from any of the service clients
            // in the AWS SDK for .NET, for X-Ray tracing. Alternatively, you can
            // register each individual service being used in this function - DynamoDB,
            // S3, and Polly.
            AWSSDKHandler.RegisterXRayForAllServices();

            // create the response object, the error code is 500 unless manually set to a success
            var apiResponse = new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" },
                    { "Access-Control-Allow-Origin", "*" }
                }
            };

            try
            {
                var userId = apigProxyEvent.RequestContext.Authorizer.Claims["cognito:username"];
                var noteId = apigProxyEvent.PathParameters["id"];
                var voiceId = JsonDocument.Parse(apigProxyEvent.Body).RootElement.GetProperty("VoiceId").GetString();

                var tableName = Environment.GetEnvironmentVariable("TABLE_NAME");
                var mp3Bucket = Environment.GetEnvironmentVariable("MP3_BUCKET_NAME");

                // Add X-Ray annotations to the trace
                AddAnnotation(userId, noteId, voiceId);

                // Get the note text from the database
                var text = await GetNote(tableName, userId, noteId);

                // Save a MP3 file locally with the output from Polly
                var filePath = await CreateMP3File(noteId, text, voiceId);

                // Host the file on S3 that is accessed by a pre-signed url
                var signedUrl = await HostFileOnS3(filePath, mp3Bucket, userId, noteId);

                apiResponse.Body = JsonSerializer.Serialize(signedUrl);
                apiResponse.StatusCode = 200;
            }
            catch (AmazonPollyException e)
            {
                context.Logger.Log($"Polly exception: {e.Message}");
                apiResponse.Body = e.Message;
            }
            catch (AmazonS3Exception e)
            {
                context.Logger.Log($"S3 exception: {e.Message}");
                apiResponse.Body = e.Message;
            }
            catch (AmazonDynamoDBException e)
            {
                context.Logger.Log($"DynamoDB exception: {e.Message}");
                apiResponse.Body = e.Message;
            }
            catch (Exception e)
            {
                context.Logger.Log($"Exception: {e.Message}");
                apiResponse.Body = JsonSerializer.Serialize(e);
            }

            return apiResponse;
        }

        public async Task<string> GetNote(string tableName, string userId, string noteId)
        {
            var ddbClient = new AmazonDynamoDBClient();
            var table = Table.LoadTable(ddbClient, tableName);
            var item = await table.GetItemAsync(userId, Int32.Parse(noteId));
            return item["Note"];
        }

        public async Task<string> CreateMP3File(string noteId, string text, string voiceId)
        {
            var pollyClient = new AmazonPollyClient();
            var request = new SynthesizeSpeechRequest
            {
                OutputFormat = OutputFormat.Mp3,
                Text = text,
                VoiceId = voiceId
            };
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

            return s3Client.GetPreSignedURL(urlRequest);
        }
    }
}
