using Microsoft.Extensions.Configuration;
using NewsReaderHTTP.Models;
using NewsReaderHTTP.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NewsReaderHTTP
{
    internal class Program
    {
        private static List<Article> lastNewsArticles = new();
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            string apiKey = Environment.GetEnvironmentVariable("NewsApiKey",EnvironmentVariableTarget.User);
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new Exception("API-ключ не найден");
            }
            Console.WriteLine("API-ключ загружен");

            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Add("User-Agent", "NewsReaderExamenApp/1.0");

                    var newServis = new NewsService(httpClient, apiKey);
                    var storageServis = new StorageService();

                    while (true)
                    {
                        Console.Clear();
                        Console.WriteLine("=======================================");
                        ColorNewsBlue("                NEWS READER");
                        Console.WriteLine("=======================================");

                        Console.WriteLine("================= МЕНЮ ================");
                        Console.WriteLine(" 1. Просмотреть топ-новости по категории");
                        Console.WriteLine(" 2. Поиск новостей по ключевому слову");
                        Console.WriteLine(" 3. Сохрпнить последние статьи с новостями ");
                        Console.WriteLine(" 4. Просмотр сохраненных новостей");

                        Console.WriteLine(" 0. Выход\n");
                        ColorNewsCyan(" Выбор >> ");

                        switch (Console.ReadLine())
                        {
                            case "1": await TopNewsMenu(newServis); break;
                            case "2":
                                Console.Write(" ВВедите ключевое слово для поиска: ");
                                string searchWord = Console.ReadLine().ToLower();
                                if(string.IsNullOrWhiteSpace(searchWord))
                                {
                                    Console.WriteLine(" Поле не может быть пустым");
                                    return;
                                }
                                else
                                {
                                    await SearchNewsByKeyword(newServis, searchWord); break;
                                }                                    
                            case "3": await SaveLastArticles(storageServis); break;
                            case "4": await ViewSavedArticles(storageServis); break;

                            case "0":
                                Console.WriteLine(" ВЫХОД");
                                return;
                            default:
                                Console.WriteLine("Неверная команда");
                                break;
                        }
                    }
                }
            }
            catch (HttpRequestException)
            {
                Console.WriteLine("Нет соединения");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
            }
        }
        //-------- ТОР --------------
        static async Task TopNewsMenu(NewsService newsService)
        {
            while (true)
            {
                Console.WriteLine("\n 1. Technology\n 2. Sports\n 3. Health\n 4. Science\n 5. Business\n ");
                Console.WriteLine(" 0. Выход\n");
                Console.Write(" Выбор >> ");

                switch (Console.ReadLine())
                {
                    case "1": await ShowNews(newsService, "technology"); break;
                    case "2": await ShowNews(newsService, "sports"); break;
                    case "3": await ShowNews(newsService, "health");break;
                    case "4": await ShowNews(newsService, "science");break;
                    case "5": await ShowNews(newsService, "business"); break;
                    case "0":
                        Console.WriteLine(" ВЫХОД");
                        return;
                    default:                        
                        Console.WriteLine("Неверная команда");                       
                        break;
                }
            }
        }
        static async Task ShowNews(NewsService newsService, string category)
        {
            var articles = await newsService.GetNewsByCategory(category);
            Console.WriteLine($"\n Получено новостей: {articles.Count}");

            lastNewsArticles = articles;

            int counter = 1;
            foreach (var art in articles)
            {
                Console.WriteLine("---------------------------------------");
                ColorNewsBlue($" {counter++}. Заголовок: {art.Title ?? "Нет заголовка"}");
                Console.WriteLine($" Описание: {art.Description ?? "Нет описания"}");
                Console.WriteLine($" Дата: {art.PublishedAt}");
                Console.WriteLine($" Ссылка: {art.Url}");
            }
        }
        static async Task SearchNewsByKeyword(NewsService newsService, string keyword)
        {
            try
            {
                var responce = await newsService.SearchNewsByKeyword(keyword);
                lastNewsArticles = responce;

                if(responce.Count == 0)
                {
                    Console.WriteLine($" По запросу \"{keyword}\" - ничего нет");
                    ColorNewsYellow(" Нажмите ENTER...");
                    Console.ReadLine();
                    return;
                }

                var currentData = DateTime.UtcNow;
                var day7Ago = currentData.AddDays(-7);
                responce = responce.Where(a => a.PublishedAt >= day7Ago).ToList();
                if (responce.Count == 0)
                {
                    Console.WriteLine("Новостей за последние 7 дней нет");
                    return;
                }

                int paginaz = 10;
                int page = (int)Math.Ceiling(responce.Count/ (double)paginaz);
                int globalIndex = 1;
                for (int i = 1; i <= page;i++)
                {
                    var startIdx = (i - 1) * paginaz;                    
                    var currentArticle = responce.Skip(startIdx).Take(paginaz).ToList();

                    foreach (var article in currentArticle)
                    {
                        ColorNewsBlue($" {globalIndex++}. {article.Title} - {article.PublishedAt:d}");
                        ColorNewsGray($"  {article.Description}\n {article.Url} ");
                        ColorNewsCyan("\n---------------------------------------\n");
                    }
                    if (i < paginaz)
                    {
                        ColorNewsYellow($"\t\t [{i}]...[{paginaz}] \n");
                        ColorNewsYellow($" Нажмите \"N\" - для выхода и \"Y\" - для перехода к следующей странице ");
                        
                        var nextPage = Console.ReadLine()?.ToLower();
                        if (nextPage == "n") break;
                        if (nextPage != "y")
                        {
                            Console.WriteLine("Неправильная команда");
                            break;
                        }
                    }
                }
            }
            catch (HttpRequestException)
            {
                Console.WriteLine("Нет соединения");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
            }
        }
        static async Task ViewSavedArticles(StorageService storageService)
        {
            try
            {
                var article = await storageService.LoadArticle();

                if (article.Count == 0)
                {
                    Console.WriteLine("\nERROR: статьи отсутствуют");
                    return;
                }

                int globalIndex = 1;
                foreach (var art in article)
                {
                    ColorNewsBlue($"{globalIndex++}. {art.Title} - {art.PublishedAt:d}");
                    ColorNewsGray($"  {art.Description}\n {art.Url} ");
                    Console.WriteLine("\n---------------------------------------");
                }

                ColorNewsCyan("Нажмите Enter...");
                Console.ReadLine();                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nОшибка: {ex.Message}");
            }
        }
        static async Task SaveLastArticles(StorageService storageService)
        {
            if(lastNewsArticles.Count == 0)
            {
                Console.WriteLine("Нет сохраненных статтей");
                return;
            }
            try
            {
                await storageService.SaveArticle(lastNewsArticles);
                ColorNewsGreen($"Сохранено {lastNewsArticles.Count} статей");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nОшибка сохранения: {ex.Message}");
            }
        }
        //---------------------------
        static void ColorNewsBlue(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(msg);
            Console.ResetColor();

        }
        static void ColorNewsCyan(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(msg);
            Console.ResetColor();
        }
        static void ColorNewsGray(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(msg);
            Console.ResetColor();
        }
        static void ColorNewsYellow(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(msg);
            Console.ResetColor();
        }
        static void ColorNewsGreen(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(msg);
            Console.ResetColor();
        }


    }
}
