using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace webapp.DAL.Tools
{
    public class ImageHostService
    {
        private static string FreeImageHostApiKey = "6d207e02198a847aa98d0a2a901485a5";
        public static async Task<string> uploadImage(IFormFile image)
        {
            if (image == null || image.Length == 0)
            {
                return "";
            }

            if (image.Length > 1 * 1024 * 1024) // 1 MB limit
            {
                throw new ArgumentException("File size exceeds the 1 MB limit.");
            }

            var client = new HttpClient();
            using var content = new MultipartFormDataContent();
            using var fileStream = image.OpenReadStream();
            using var streamContent = new StreamContent(fileStream);

            streamContent.Headers.ContentType = new MediaTypeHeaderValue(image.ContentType);
            content.Add(streamContent, "source", image.FileName);

            var response = await client.PostAsync($"https://freeimage.host/api/1/upload?key={FreeImageHostApiKey}&action=upload", content);

            if (!response.IsSuccessStatusCode)
            {
                return "Error uploading to freeimage.host";
            }

            var responseData = await response.Content.ReadAsStringAsync();
            using (JsonDocument doc = JsonDocument.Parse(responseData))
            {
                var root = doc.RootElement;
                return root.GetProperty("image").GetProperty("url").ToString();
            }
        }
    }
}
