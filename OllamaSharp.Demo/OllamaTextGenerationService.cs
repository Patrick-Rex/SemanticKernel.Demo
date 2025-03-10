using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.TextGeneration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OllamaSharp.Demo
{
    public class OllamaTextGenerationService : Microsoft.SemanticKernel.TextGeneration.ITextGenerationService
    {
        /// <summary>
        /// 模型URL
        /// </summary>
        public string ModelUrl { get; set; }
        /// <summary>
        /// 模型名称
        /// </summary>
        public string ModelName { get; set; }
        private readonly HttpClient _httpClient = new HttpClient();

        public IReadOnlyDictionary<string, object?> Attributes => new Dictionary<string, object?>();

        public IAsyncEnumerable<StreamingTextContent> GetStreamingTextContentsAsync(string prompt, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<IReadOnlyList<TextContent>> GetTextContentsAsync(string prompt, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, CancellationToken cancellationToken = default)
        {
            try
            {
                Console.WriteLine($"OllamaTextGenerationService: 发送请求到 {ModelUrl}, 模型: {ModelName}");
                Console.WriteLine($"OllamaTextGenerationService: 提示: {prompt}");
                
                // 创建请求正文
                var requestBody = new
                {
                    model = ModelName,
                    prompt = prompt,
                    temperature = 0.7,
                    stream = false
                };
                
                var jsonContent = JsonSerializer.Serialize(requestBody);
                Console.WriteLine($"请求正文: {jsonContent}");
                
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                
                // 判断ModelUrl是否已经包含了完整的API路径
                string apiUrl = ModelUrl;
                if (!apiUrl.Contains("/api/generate"))
                {
                    apiUrl = apiUrl.TrimEnd('/') + "/api/generate";
                }
                
                Console.WriteLine($"发送请求到: {apiUrl}");
                
                var response = await _httpClient.PostAsync(apiUrl, content, cancellationToken);
                Console.WriteLine($"HTTP状态码: {response.StatusCode}");
                
                response.EnsureSuccessStatusCode();
                
                var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
                Console.WriteLine($"收到响应: {responseJson}");
                
                // 解析JSON响应
                using var doc = JsonDocument.Parse(responseJson);
                
                string textResponse = "";
                
                // 尝试获取response字段（标准Ollama API）
                if (doc.RootElement.TryGetProperty("response", out var responseElement))
                {
                    textResponse = responseElement.GetString() ?? "";
                }
                
                Console.WriteLine($"最终文本内容: {textResponse}");
                
                if (string.IsNullOrEmpty(textResponse))
                {
                    Console.WriteLine("警告: 文本生成响应为空");
                }
                
                return new List<TextContent>
                {
                    new TextContent(textResponse)
                };
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"HTTP请求异常: {httpEx.Message}");
                Console.WriteLine($"状态码: {httpEx.StatusCode}");
                throw;
            }
        }
    }
}
