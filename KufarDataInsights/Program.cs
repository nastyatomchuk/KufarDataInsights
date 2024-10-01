using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

class Program
{
    static async Task Main(string[] args)
    {
        // URL для получения данных о квартирах в Минске
        string apiUrl = "https://api.kufar.by/search-api/v2/search/rendered-paginated?cat=1010&cur=USD&gtsy=country-belarus~province-minsk~locality-minsk&lang=ru&size=30&typ=sell";

        // Минимальная цена за квадратный метр
        decimal minPricePerM2 = 1000; // Установите желаемый порог

        // Создаем HttpClient для выполнения запроса
        using (HttpClient client = new HttpClient())
        {
            try
            {
                // Получаем ответ от API
                var jsonResponse = await client.GetStringAsync(apiUrl);
                // Парсим JSON-ответ
                var data = JObject.Parse(jsonResponse);
                // Получаем список объявлений
                var ads = data["ads"];

                // Словари для хранения цен за квадратный метр
                var pricesByFloor = new Dictionary<int, List<decimal>>();
                var pricesByRooms = new Dictionary<int, List<decimal>>();
                var pricesByMetro = new Dictionary<string, List<decimal>>();

                // Проходим по каждому объявлению
                foreach (var ad in ads)
                {
                    // Получаем этаж квартиры
                    var floorInfo = ad["ad_parameters"]?.FirstOrDefault(p => p["p"].ToString() == "floor");
                    // Получаем цену за квадратный метр
                    var priceInfo = ad["ad_parameters"]?.FirstOrDefault(p => p["p"].ToString() == "square_meter");
                    // Получаем количество комнат
                    var roomsInfo = ad["ad_parameters"]?.FirstOrDefault(p => p["p"].ToString() == "rooms");
                    // Получаем информацию о метро
                    var metroInfo = ad["ad_parameters"]?.FirstOrDefault(p => p["p"].ToString() == "metro");

                    // Проверяем, что мы нашли этаж, цену, количество комнат и метро
                    if (floorInfo != null && priceInfo != null && roomsInfo != null)
                    {
                        // Извлекаем значения
                        int floor = floorInfo["v"]?.First.ToObject<int>() ?? 0;
                        decimal pricePerM2 = priceInfo["v"]?.ToObject<decimal>() ?? 0;
                        int rooms = roomsInfo["v"]?.ToObject<int>() ?? 0;

                        // Игнорируем квартиры с ценой ниже установленного порога
                        if (pricePerM2 < minPricePerM2) continue;

                        // Добавляем цену за квадратный метр в словарь по этажам
                        if (!pricesByFloor.ContainsKey(floor))
                        {
                            pricesByFloor[floor] = new List<decimal>();
                        }
                        pricesByFloor[floor].Add(pricePerM2);

                        // Добавляем цену за квадратный метр в словарь по количеству комнат
                        if (!pricesByRooms.ContainsKey(rooms))
                        {
                            pricesByRooms[rooms] = new List<decimal>();
                        }
                        pricesByRooms[rooms].Add(pricePerM2);

                        // Игнорируем квартиры без указанной станции метро
                        if (metroInfo != null && metroInfo["vl"] != null && metroInfo["vl"].Any())
                        {
                            string metroStation = metroInfo["vl"].First.ToString();
                            if (!pricesByMetro.ContainsKey(metroStation))
                            {
                                pricesByMetro[metroStation] = new List<decimal>();
                            }
                            pricesByMetro[metroStation].Add(pricePerM2);
                        }
                    }
                }

                // Выводим заголовок таблицы для этажей
                Console.WriteLine("Этаж | Средняя цена за м²");
                Console.WriteLine("-------------------------");

                // Проходим по каждому этажу и считаем среднюю цену
                foreach (var entry in pricesByFloor.OrderBy(e => e.Key))
                {
                    decimal averagePrice = entry.Value.Count > 0 ? entry.Value.Average() : 0;
                    Console.WriteLine($"{entry.Key,3} | {averagePrice:F2} $");
                }

                // Выводим заголовок таблицы для количества комнат
                Console.WriteLine("\nКомнаты | Средняя цена за м²");
                Console.WriteLine("----------------------------");

                // Проходим по каждому количеству комнат и считаем среднюю цену
                foreach (var entry in pricesByRooms.OrderBy(e => e.Key))
                {
                    decimal averagePrice = entry.Value.Count > 0 ? entry.Value.Average() : 0;
                    Console.WriteLine($"{entry.Key,7} | {averagePrice:F2} $");
                }

                // Выводим заголовок таблицы для станции метро
                Console.WriteLine("\nМетро         | Средняя цена за м²");
                Console.WriteLine("-------------------------------");

                // Проходим по каждой станции метро и считаем среднюю цену
                foreach (var entry in pricesByMetro.OrderBy(e => e.Key))
                {
                    decimal averagePrice = entry.Value.Count > 0 ? entry.Value.Average() : 0;
                    Console.WriteLine($"{entry.Key,-15} | {averagePrice:F2} $");
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("Ошибка при выполнении запроса: " + e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Произошла ошибка: " + e.Message);
            }
        }

        Console.ReadLine();
    }
}