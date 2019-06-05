using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Shared.Extensions
{
    public static class HttpClientExtensions
    {
        public static Task<HttpResponseMessage> PostAsJsonAsync<T>(this HttpClient httpClient, string url, T data)
        {
            var dataAsString = JsonConvert.SerializeObject(data, Formatting.Indented,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Objects,
                    TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
                });

#if DEBUG
            Console.WriteLine();
            Console.WriteLine($"DEBUG Serialized {typeof(T).Name}: {dataAsString}");
            Console.WriteLine();
#endif

            var content = new StringContent(dataAsString);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return httpClient.PostAsync(url, content);
        }

        public static Task<HttpResponseMessage> PutAsJsonAsync<T>(this HttpClient httpClient, string url, T data)
        {
            var dataAsString = JsonConvert.SerializeObject(data, Formatting.Indented,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Objects,
                    TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
                });

#if DEBUG
            Console.WriteLine();
            Console.WriteLine($"DEBUG Serialized {typeof(T).Name}: {dataAsString}");
            Console.WriteLine();
#endif

            var content = new StringContent(dataAsString);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return httpClient.PutAsync(url, content);
        }

        public static Task<HttpResponseMessage> DeleteByJsonAsync<T>(this HttpClient httpClient, string url, T data)
        {
            var dataAsString = JsonConvert.SerializeObject(data, Formatting.Indented,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Objects,
                    TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
                });

#if DEBUG
            Console.WriteLine();
            Console.WriteLine($"DEBUG Serialized {typeof(T).Name}: {dataAsString}");
            Console.WriteLine();
#endif

            var content = new StringContent(dataAsString);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            HttpRequestMessage request = new HttpRequestMessage()
            {
                Content = content,
                Method = HttpMethod.Delete,
                RequestUri = new Uri(httpClient.BaseAddress + url)
            };
            return httpClient.SendAsync(request);
        }

        public static Task<HttpResponseMessage> GetByJsonAsync<T>(this HttpClient httpClient, string url, T data)
        {
            var dataAsString = JsonConvert.SerializeObject(data, Formatting.Indented,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Objects,
                    TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
                });

#if DEBUG
            Console.WriteLine();
            Console.WriteLine($"DEBUG Serialized {typeof(T).Name}: {dataAsString}");
            Console.WriteLine();
#endif

            var content = new StringContent(dataAsString);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            HttpRequestMessage request = new HttpRequestMessage()
            {
                Content = content,
                Method = HttpMethod.Get,
                RequestUri = new Uri(httpClient.BaseAddress + url)
            };
            return httpClient.SendAsync(request);
        }
    }
}