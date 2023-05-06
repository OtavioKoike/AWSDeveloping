using System;

namespace DynamoDBOperations
{
    class Program
    {
        const string createTable = "create-table";
        const string loadData = "load-data";
        const string paginateData = "paginate-data";
        const string queryData = "query-data";
        const string updateData = "update-data";
        const string highlevelApi = "highlevel-api";

        static void Main(string[] args)
        {
            var operation = args.Length > 0 ? args[0].ToLowerInvariant() : "";
            if (string.IsNullOrEmpty(operation))
            {
                operation = PromptUserForOperation();
            }

            switch (operation)
            {
                case createTable:
                    new CreateTableTask().Run().Wait();
                    break;

                case loadData:
                    new LoadDataTask().Run().Wait();
                    break;

                case queryData:
                    new QueryDataTask().Run().Wait();
                    break;

                case paginateData:
                    new PaginateDataTask().Run().Wait();
                    break;

                case updateData:
                    new UpdateDataTask().Run().Wait();
                    break;

                case highlevelApi:
                    new HighLevelAPITask().Run().Wait();
                    break;

                default:
                    throw new ArgumentException($"Unknown operation. Expected {createTable}, {loadData}, {queryData}, {paginateData}, {updateData}, or {highlevelApi} options.");
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        private static string PromptUserForOperation()
        {
            var options = new[] { createTable, loadData, queryData, paginateData, updateData, highlevelApi };

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
                        selection = options[index - 1];
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
