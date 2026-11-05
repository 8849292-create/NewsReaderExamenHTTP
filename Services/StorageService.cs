using NewsReaderHTTP.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace NewsReaderHTTP.Services
{
    public class StorageService
    {        
        private readonly string FilePath;
        
        public StorageService()
        {
            string dataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            Directory.CreateDirectory(dataDirectory);
            FilePath = Path.Combine(dataDirectory,"saved_articles.json");
            Console.WriteLine("Путь к файлу: " + FilePath);
        }
        public async Task SaveArticle(List<Article> articles)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };

            await File.WriteAllTextAsync(FilePath, JsonSerializer.Serialize(articles, options));
        }       
        public async Task<List<Article>> LoadArticle()
        {
            try
            {
                if (!File.Exists(FilePath)) return new List<Article>();

                string json = await File.ReadAllTextAsync(FilePath);
                if (string.IsNullOrWhiteSpace(json)) return new List<Article>();

                return JsonSerializer.Deserialize<List<Article>>(json) ?? new List<Article>();
            }
            catch (JsonException ex)
            {
                throw new Exception("Некорректный JSON", ex);
            }
            catch (IOException ex)
            {
                throw new Exception($"ERROR: {ex.Message}");
            }
        }
    }
}
