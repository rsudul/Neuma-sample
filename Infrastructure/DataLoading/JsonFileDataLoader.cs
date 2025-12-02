using System;
using System.IO;
using System.Text.Json;
using Neuma.Core.DataLoading;

namespace Neuma.Infrastructure.DataLoading
{
    public sealed class JsonFileDataLoader<T> : IDataLoader<T> where T : class
    {
        private readonly JsonSerializerOptions _options;

        public JsonFileDataLoader(JsonSerializerOptions? options = null)
        {
            _options = options ?? new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                PropertyNameCaseInsensitive = true
            };
        }

        public T Load(string resourceId)
        {
            if (string.IsNullOrWhiteSpace(resourceId))
            {
                throw new ArgumentException("Resource id cannot be null or whitespace.", nameof(resourceId));
            }

            if (!File.Exists(resourceId))
            {
                throw new FileNotFoundException($"JsonFileDataLoader could not find file '{resourceId}'.", resourceId);
            }

            string json;

            try
            {
                json = File.ReadAllText(resourceId);
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to read JSON file '{resourceId}'.", ex);
            }

            if (string.IsNullOrWhiteSpace(json))
            {
                throw new InvalidDataException($"JSON file '{resourceId}' is empty or whitespace.");
            }

            try
            {
                var result = JsonSerializer.Deserialize<T>(json, _options);

                if (result == null)
                {
                    throw new InvalidOperationException($"Deserialization of '{resourceId}' returned null.");
                }

                return result;
            }
            catch (JsonException ex)
            {
                throw new InvalidDataException($"Invalid JSON format in '{resourceId}'.", ex);
            }
        }
    }
}

