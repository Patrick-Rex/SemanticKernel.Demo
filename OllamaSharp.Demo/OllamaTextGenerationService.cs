using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.TextGeneration;
using OllamaSharp.ApiClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OllamaSharp.Demo
{
    public class OllamaTextGenerationService : Microsoft.SemanticKernel.TextGeneration.ITextGenerationService
    {
        // public property for the model url endpoint
        public string ModelUrl { get; set; }
        public string ModelName { get; set; }

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
                
                var ollama = new OllamaApiClient(ModelUrl, ModelName);
                
                // 使用Completion.Create而不是Generate，避免使用不存在的方法
                var response = await ollama.Completion.Create(new CompletionRequest
                {
                    Prompt = prompt,
                    Stream = false
                });
                
                if (response == null)
                {
                    Console.WriteLine("OllamaTextGenerationService: API返回了null响应");
                    return new List<TextContent>();
                }
                
                Console.WriteLine($"OllamaTextGenerationService: 收到响应: {response.Response}");
                
                return new List<TextContent>
                {
                    new TextContent(response.Response)
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OllamaTextGenerationService异常: {ex.Message}");
                Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                
                // 重新抛出异常，让主程序处理
                throw;
            }
        }
    }
}
