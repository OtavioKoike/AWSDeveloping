using System;

namespace S3Operations
{
    class Program
    {
        const string createBucket = "create-bucket";
        const string createObject = "create-object";
        const string createS3Website = "create-s3-website";
        const string convertObject = "convert-object";

        static void Main(string[] args)
        {
            var operation = args.Length > 0 ? args[0].ToLowerInvariant() : "";
            if (string.IsNullOrEmpty(operation))
            {
                operation = PromptUserForOperation();
            }

            switch (operation)
            {
                case createBucket:
                    new CreateBucketTask().Run().Wait();
                    break;

                case createObject:
                    new CreateObjectTask().Run().Wait();
                    break;

                case convertObject:
                    new ConvertObjectTask().Run().Wait();
                    break;

                case createS3Website:
                    new CreateS3WebsiteTask().Run().Wait();
                    break;

                default:
                    throw new ArgumentException($"Unknown operation. Expected {createBucket}, {createObject}, {convertObject}, or {createS3Website} options.");
            }

            Console.WriteLine("\nPress any key to exit...");
			Console.ReadKey();
        }

        private static string PromptUserForOperation()
        {
            var options = new[] { createBucket, createObject, convertObject, createS3Website };

            string selection = "";
            do
            {
                Console.WriteLine("Select the task to run.");
                var optionIndex = 1;
                foreach (var op in options)
                {
                    Console.WriteLine($"{optionIndex++}: {op}");
                }

                var input = Console.ReadLine();
                if (Int32.TryParse(input, out int index))
                {
                    if (index > 0 && index <= options.Length)
                    {
                        selection = options[index-1];
                    }
                    else
                    {
                        Console.WriteLine("Invalid selection");
                    }
                }
            } while (string.IsNullOrEmpty(selection));

            return selection;
        }
    }
}
