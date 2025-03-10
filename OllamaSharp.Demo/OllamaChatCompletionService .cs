using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using OllamaSharp;
using OllamaSharp.Models.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OllamaSharp.Demo
{
    public class OllamaChatCompletionService : IChatCompletionService
    {
        // public property for the model url endpoint
        public string ModelUrl { get; set; }
        public string ModelName { get; set; }
        private readonly HttpClient _httpClient = new HttpClient();

        public IReadOnlyDictionary<string, object?> Attributes => new Dictionary<string, object?>();

        public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(ChatHistory chatHistory, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, CancellationToken cancellationToken = default)
        {
            try
            {
                Console.WriteLine($"OllamaChatCompletionService: 发送请求到 {ModelUrl}, 模型: {ModelName}");
                
                // 创建消息列表
                var messages = new List<object>();
                
                foreach (var message in chatHistory)
                {
                    string role = message.Role.ToString().ToLower();
                    
                    // 添加消息到列表
                    messages.Add(new
                    {
                        role = role,
                        content = message.Content
                    });
                    
                    Console.WriteLine($"添加消息: 角色={role}, 内容={message.Content}");
                }
                
                // 创建请求正文
                var requestBody = new
                {
                    model = ModelName,
                    messages = messages,
                    temperature = 0.7,
                    stream = false
                };
                
                var jsonContent = JsonSerializer.Serialize(requestBody);
                Console.WriteLine($"请求正文: {jsonContent}");
                
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                
                // 判断ModelUrl是否已经包含了完整的API路径
                string apiUrl = ModelUrl;
                if (!apiUrl.Contains("/v1/chat/completions"))
                {
                    apiUrl = apiUrl.TrimEnd('/') + "/api/chat";
                }
                
                Console.WriteLine($"发送请求到: {apiUrl}");
                
                var response = await _httpClient.PostAsync(apiUrl, content, cancellationToken);
                Console.WriteLine($"HTTP状态码: {response.StatusCode}");
                
                response.EnsureSuccessStatusCode();
                
                var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
                Console.WriteLine($"收到响应: {responseJson}");
                
                // 解析JSON响应
                using var doc = JsonDocument.Parse(responseJson);
                
                string chatResponse = "";
                
                // 尝试获取response字段（老版本Ollama API）
                if (doc.RootElement.TryGetProperty("response", out var responseElement))
                {
                    chatResponse = responseElement.GetString() ?? "";
                }
                // 尝试获取message/content字段（新版本Ollama API）
                else if (doc.RootElement.TryGetProperty("message", out var messageElement) && 
                         messageElement.TryGetProperty("content", out var contentElement))
                {
                    chatResponse = contentElement.GetString() ?? "";
                }
                // 尝试获取choices/message/content字段（OpenAI兼容API）
                else if (doc.RootElement.TryGetProperty("choices", out var choicesElement) && 
                         choicesElement.GetArrayLength() > 0)
                {
                    var firstChoice = choicesElement[0];
                    if (firstChoice.TryGetProperty("message", out var choiceMessageElement) && 
                        choiceMessageElement.TryGetProperty("content", out var choiceContentElement))
                    {
                        chatResponse = choiceContentElement.GetString() ?? "";
                    }
                }
                
                Console.WriteLine($"最终回复内容: {chatResponse}");
                
                if (string.IsNullOrEmpty(chatResponse))
                {
                    Console.WriteLine("警告: 聊天响应为空");
                }
                
                // 创建并返回包含响应的新列表
                return new List<ChatMessageContent>
                {
                    new ChatMessageContent(AuthorRole.Assistant, chatResponse)
                };
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"HTTP请求异常: {httpEx.Message}");
                Console.WriteLine($"状态码: {httpEx.StatusCode}");
                throw;
            }
        }

        public IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(ChatHistory chatHistory, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
