using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.TextGeneration;
using OllamaSharp.Demo;
using System.Net.Http;
using System.Text.Json;

try
{
    Console.WriteLine("程序开始运行...");
    Console.WriteLine("按任意键继续...");
    Console.ReadKey();

    // 使用本地Ollama服务
    var ollamaChat = new OllamaChatCompletionService();
    ollamaChat.ModelUrl = "http://localhost:11434";  // 标准Ollama API端点
    ollamaChat.ModelName = "llama3.2:1b";  // 使用可用的模型

    var ollamaText = new OllamaTextGenerationService();
    ollamaText.ModelUrl = "http://localhost:11434";  // 标准Ollama API端点
    ollamaText.ModelName = "llama3.2:1b";  // 使用可用的模型

    Console.WriteLine("服务初始化完成...");

    // 提示词模板字典
    Dictionary<string, string> promptTemplates = new Dictionary<string, string>
    {
        { "通用回答", "请以专业、友好的方式回答以下问题：{{input}}" },
        { "中文翻译", "请将以下英文翻译成中文：{{input}}" },
        { "代码解释", "请解释以下代码的功能和实现原理：\n```\n{{input}}\n```" },
        { "创意写作", "请根据以下主题，创作一段有创意的短文：{{input}}" },
        { "摘要生成", "请对以下文本进行概括总结，提炼出核心观点：\n{{input}}" }
    };

    // semantic kernel builder
    var builder = Kernel.CreateBuilder();
    builder.Services.AddKeyedSingleton<IChatCompletionService>("ollamaChat", ollamaChat);
    builder.Services.AddKeyedSingleton<ITextGenerationService>("ollamaText", ollamaText);
    var kernel = builder.Build();

    Console.WriteLine("Kernel构建完成...");
    Console.WriteLine("按任意键继续执行API调用...");
    Console.ReadKey();

    // 检查并更新模型名称
    await UpdateModelNameIfNeeded(ollamaChat, ollamaText);

    // 文本生成 - 使用提示词模板
    Console.WriteLine("\n选择提示策略：");
    Console.WriteLine("1. 直接提问 (Direct)");
    Console.WriteLine("2. 思维链 (Chain-of-Thought)");
    Console.WriteLine("3. 思考-行动-观察 (ReAct)");
    Console.WriteLine("4. 思维树 (Tree of Thoughts)");
    Console.WriteLine("5. 自洽性检查 (Self-Consistency)");
    Console.WriteLine("6. 模板库 (Templates)");
    Console.Write("请选择提示策略编号: ");

    if (int.TryParse(Console.ReadLine(), out int strategyChoice) && strategyChoice >= 1 && strategyChoice <= 6)
    {
        Console.Write("请输入您的问题或提示: ");
        string basePrompt = Console.ReadLine() ?? "什么是人工智能？";

        var textGen = kernel.GetRequiredService<ITextGenerationService>();
        string finalPrompt = basePrompt;

        // 应用提示词策略
        if (strategyChoice == 6)
        {
            // 使用模板库
            Console.WriteLine("\n选择提示词模板：");
            for (int i = 0; i < promptTemplates.Keys.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {promptTemplates.Keys.ElementAt(i)}");
            }
            Console.Write("请输入模板编号: ");
            if (int.TryParse(Console.ReadLine(), out int templateIndex) && templateIndex > 0 && templateIndex <= promptTemplates.Count)
            {
                string templateName = promptTemplates.Keys.ElementAt(templateIndex - 1);
                string template = promptTemplates[templateName];
                finalPrompt = template.Replace("{{input}}", basePrompt);
                Console.WriteLine($"使用模板 '{templateName}'");
            }
        }
        else
        {
            // 使用提示词工程策略
            PromptEngineering.PromptStrategy strategy = PromptEngineering.PromptStrategy.Direct;

            switch (strategyChoice)
            {
                case 1:
                    strategy = PromptEngineering.PromptStrategy.Direct;
                    break;
                case 2:
                    strategy = PromptEngineering.PromptStrategy.CoT;
                    break;
                case 3:
                    strategy = PromptEngineering.PromptStrategy.ReAct;
                    break;
                case 4:
                    strategy = PromptEngineering.PromptStrategy.TreeOfThoughts;
                    break;
                case 5:
                    strategy = PromptEngineering.PromptStrategy.SelfConsistency;
                    break;
            }

            finalPrompt = PromptEngineering.ApplyStrategy(basePrompt, strategy);
        }

        Console.WriteLine($"\n最终提示词：\n{finalPrompt}");
        Console.WriteLine("\n正在发送文本生成请求...");

        try
        {
            var textResponse = await textGen.GetTextContentsAsync(finalPrompt);

            // 检查响应是否为空
            if (textResponse == null || textResponse.Count == 0)
            {
                Console.WriteLine("错误：文本生成返回为空");
            }
            else
            {
                Console.WriteLine($"收到 {textResponse.Count} 条文本结果");
                Console.WriteLine("\n文本生成结果：\n" + textResponse[0].Text);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"文本生成错误: {ex.Message}");
        }
    }
    else
    {
        Console.WriteLine("无效的选择，使用默认提示词...");
    }

    Console.WriteLine("文本生成完成。按任意键继续聊天请求...");
    Console.ReadKey();

    // chat
    try
    {
        Console.WriteLine("\n正在准备聊天请求...");
        Console.WriteLine($"URL: {ollamaChat.ModelUrl}, 模型: {ollamaChat.ModelName}");

        var chat = kernel.GetRequiredService<IChatCompletionService>();
        var history = new ChatHistory();
        history.AddSystemMessage(@"你是一位专业的人工智能助手，名叫摇光。
                                请遵循以下规则：
                                1. 使用简短、直接的语言回答问题
                                2. 在回复中使用emoji表情增加亲切感 😊
                                3. 如果不确定答案，请直接承认，不要编造信息
                                4. 当解释技术概念时，先给出简单解释，再提供深入细节
                                5. 使用举例来解释复杂概念
                                6. 你的回答应该有条理、有逻辑，便于阅读
                                7. 用中文回复所有问题");
        history.AddUserMessage("嗨，你是谁？能帮我做什么？");

        Console.WriteLine("已添加系统消息和用户消息，正在发送请求...");
        Console.WriteLine("按任意键发送聊天请求...");
        Console.ReadKey();

        // print response
        var chatResult = await chat.GetChatMessageContentsAsync(history);

        // 检查响应是否为空
        if (chatResult == null)
        {
            Console.WriteLine("错误：聊天返回了null");
        }
        else if (chatResult.Count == 0)
        {
            Console.WriteLine("错误：聊天返回了空列表");
        }
        else
        {
            Console.WriteLine($"收到 {chatResult.Count} 条聊天结果");
            Console.WriteLine("聊天结果：" + chatResult[0].Content);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"聊天请求错误: {ex.Message}");
        Console.WriteLine($"异常类型: {ex.GetType().Name}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"内部异常: {ex.InnerException.Message}");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"主程序异常: {ex.Message}");
    Console.WriteLine($"异常类型: {ex.GetType().Name}");
    Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");

    // 如果有内部异常，也显示
    if (ex.InnerException != null)
    {
        Console.WriteLine($"内部异常: {ex.InnerException.Message}");
        Console.WriteLine($"内部异常类型: {ex.InnerException.GetType().Name}");
    }
}

Console.WriteLine("程序执行完成。按任意键退出...");
Console.ReadKey();


// 模型检查和更新方法
static async Task UpdateModelNameIfNeeded(OllamaChatCompletionService chatService, OllamaTextGenerationService textService)
{
    try
    {
        Console.WriteLine("验证模型是否可用...");
        using var httpClient = new HttpClient();
        var response = await httpClient.GetAsync("http://localhost:11434/api/tags");

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"发现的模型: {content}");

            // 解析可用模型列表
            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.TryGetProperty("models", out var modelsElement))
            {
                var availableModels = new List<string>();
                foreach (var model in modelsElement.EnumerateArray())
                {
                    if (model.TryGetProperty("name", out var nameElement))
                    {
                        availableModels.Add(nameElement.GetString() ?? "");
                    }
                }

                Console.WriteLine($"可用模型列表: {string.Join(", ", availableModels)}");

                // 检查当前模型是否可用
                if (availableModels.Count > 0)
                {
                    if (!availableModels.Contains(chatService.ModelName))
                    {
                        // 当前模型不可用，使用第一个可用模型
                        var newModel = availableModels[0];
                        Console.WriteLine($"模型 {chatService.ModelName} 不可用，切换到 {newModel}");
                        chatService.ModelName = newModel;
                        textService.ModelName = newModel;
                    }
                    else
                    {
                        Console.WriteLine($"模型 {chatService.ModelName} 可用，继续使用");
                    }
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"检查模型时出错: {ex.Message}");
        Console.WriteLine("继续使用默认模型...");
    }
}
