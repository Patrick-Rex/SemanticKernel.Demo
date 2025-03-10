using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.TextGeneration;
using OllamaSharp.Demo;

static async Task Main(string[] args)
{
    Console.WriteLine("程序开始运行...");
    // llama2 in Ubuntu local in WSL
    var ollamaChat = new OllamaChatCompletionService();
    ollamaChat.ModelUrl = "http://localhost:11434";
    ollamaChat.ModelName = "llama3.2:1b";
    
    var ollamaText = new OllamaTextGenerationService();
    ollamaText.ModelUrl = "http://localhost:11434";
    ollamaText.ModelName = "llama3.2:1b";
    
    // semantic kernel builder
    var builder = Kernel.CreateBuilder();
    builder.Services.AddKeyedSingleton<IChatCompletionService>("ollamaChat", ollamaChat);
    builder.Services.AddKeyedSingleton<ITextGenerationService>("ollamaText", ollamaText);
    var kernel = builder.Build();
    
    try
    {
        // text generation
        var textGen = kernel.GetRequiredService<ITextGenerationService>();
        Console.WriteLine("正在获取文本生成结果...");
        Console.WriteLine($"URL: {ollamaText.ModelUrl}, 模型: {ollamaText.ModelName}");
        
        var response = await textGen.GetTextContentsAsync("The weather in January in Toronto is usually ");
        
        // 检查响应是否为空
        if (response == null)
        {
            Console.WriteLine("错误：文本生成返回了null");
        }
        else if (response.Count == 0)
        {
            Console.WriteLine("错误：文本生成返回了空列表");
        }
        else
        {
            Console.WriteLine($"收到 {response.Count} 条文本结果");
            Console.WriteLine("文本生成结果：" + response[0].Text);
        }
        
        // chat
        Console.WriteLine("\n正在获取聊天结果...");
        Console.WriteLine($"URL: {ollamaChat.ModelUrl}, 模型: {ollamaChat.ModelName}");
        
        var chat = kernel.GetRequiredService<IChatCompletionService>();
        var history = new ChatHistory();
        history.AddSystemMessage("You are a useful assistant that replies using a funny style and emojis. Your name is Goku.");
        history.AddUserMessage("hi, who are you?");
        
        Console.WriteLine("已添加系统消息和用户消息，正在发送请求...");
        
        // print response
        var result = await chat.GetChatMessageContentsAsync(history);
        
        // 检查响应是否为空
        if (result == null)
        {
            Console.WriteLine("错误：聊天返回了null");
        }
        else if (result.Count == 0)
        {
            Console.WriteLine("错误：聊天返回了空列表");
        }
        else
        {
            Console.WriteLine($"收到 {result.Count} 条聊天结果");
            Console.WriteLine("聊天结果：" + result[0].Content);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"发生错误: {ex.Message}");
        Console.WriteLine($"异常类型: {ex.GetType().Name}");
        Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");
        
        // 如果有内部异常，也显示
        if (ex.InnerException != null)
        {
            Console.WriteLine($"内部异常: {ex.InnerException.Message}");
            Console.WriteLine($"内部异常类型: {ex.InnerException.GetType().Name}");
        }
    }
    
    Console.WriteLine("按任意键退出...");
    Console.ReadKey();
}
