using NewsReaderExamenHTTP.Models;
using NewsReaderHTTP.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace NewsReaderHTTP.Services
{
    public class NewsService
    {
        private readonly HttpClient httpClient;
        private readonly string ApiKey;
        public NewsService(HttpClient httpclient, string apikey )
        {
            httpClient = httpclient;
            ApiKey = apikey;
        }
        public async Task<List<Article>> GetNewsByCategory(string category)
        {
            var responce = await httpClient.GetAsync($"https://newsapi.org/v2/top-headlines?" +
                                                     $"category={category}&" +
                                                     $"apiKey={ApiKey}");
            responce.EnsureSuccessStatusCode();
            var content = await responce.Content.ReadAsStringAsync();

            var newsResponse = JsonSerializer.Deserialize<NewsApiResponse>(content);
            return newsResponse?.Articles ?? new List<Article>();            
        }

        public async Task<List<Article>> SearchNewsByKeyword(string keyword)
        {
            string url = $"https://newsapi.org/v2/everything?" +
                         $"q={Uri.EscapeDataString(keyword)}&" +
                         $"sortBy=publishedAt";

            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            {
                request.Headers.Add("X-Api-Key", ApiKey);

                using (var responce = await httpClient.SendAsync(request))
                {
                    responce.EnsureSuccessStatusCode();

                    var content = await responce.Content.ReadAsStringAsync();
                    var newsResponce =JsonSerializer.Deserialize<NewsApiResponse>(content);

                    return newsResponce?.Articles ?? new List<Article>();
                }
            }
        }
    }
}
