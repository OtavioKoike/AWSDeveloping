using System;
using Microsoft.Extensions.Configuration;

namespace S3Operations
{
    public abstract class ConfigSettingsReader<T>
    {
        public static T Read(string sectionKey)
        {
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();

            var configSection = configBuilder.GetSection(sectionKey);
            return configSection.Get<T>();
        }
    }
}
