using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace WebApplication9.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CatController : ControllerBase
    {
        private static HttpClient client = new HttpClient();
        private static Dictionary<int, byte[]> imageCache = new Dictionary<int, byte[]>();

        [HttpGet]
        public async Task<IActionResult> GetCatImage(string url)
        {
            try
            {
                // Отправляем GET запрос и получаем статус код
                HttpResponseMessage response = await client.GetAsync(url);
                int statusCode = (int)response.StatusCode;

                byte[] catImage;
                // Проверяем, есть ли изображение в кэше
                if (!imageCache.ContainsKey(statusCode))
                {
                    // Если изображения нет в кэше, загружаем его
                    catImage = await GetCatImageFromApi(statusCode);
                    // Кешируем изображение
                    CacheImage(statusCode, catImage);
                }
                else
                {
                    // Если изображение уже есть в кэше, используем его
                    catImage = imageCache[statusCode];
                }

                // Возвращаем изображение в ответе
                return File(catImage, "image/jpeg");
            }
            catch (Exception ex)
            {
                // Обрабатываем возможные ошибки
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private async Task<byte[]> GetCatImageFromApi(int statusCode)
        {
            // Отправляем запрос к https://http.cat/ для получения изображения по статус коду
            HttpResponseMessage response = await client.GetAsync($"https://http.cat/status/{statusCode}");
            response.EnsureSuccessStatusCode();
            // Читаем содержимое ответа в массив байтов
            return await response.Content.ReadAsByteArrayAsync();
        }

        private void CacheImage(int statusCode, byte[] image)
        {
            // Кешируем изображение на определенное время в другом потоке
            Task.Run(() =>
            {
                // Время кэширования можно настроить по вашему желанию
                Thread.Sleep(TimeSpan.FromMinutes(30)); // Например, на 30 минут
                imageCache.Remove(statusCode);
            });
            // Добавляем изображение в кэш
            imageCache[statusCode] = image;
        }
    }
}
