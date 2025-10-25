using System.Text;

namespace Reviewer
{
    public class LlmClient
    {
        private readonly string _baseUrl;
        private readonly HttpClient _http;

        public LlmClient(string baseUrl)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _http = new HttpClient();
        }

        public async Task<string> GetCompletionJsonAsync(string prompt)
        {
            // Example for Ollama: POST /api/generate with model & prompt
            var payload = new
            {
                model = "code-llama-7b", // choose local model
                prompt = prompt,
                max_tokens = 1500
            };
            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            var resp = await _http.PostAsync($"{_baseUrl}/api/generate", new StringContent(json, Encoding.UTF8, "application/json"));
            resp.EnsureSuccessStatusCode();
            var body = await resp.Content.ReadAsStringAsync();
            // With some local servers the actual text is in a field — adapt as needed
            return body;
        }
    }
}
