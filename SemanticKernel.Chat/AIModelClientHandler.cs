using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SemanticKernel.Chat.Entity;
using SemanticKernel.Chat.Enum;

namespace SemanticKernel.Chat
{
    public static class AIModelClientHandler
    {
        /// <summary>
        /// 创建Kernel，根据提供商名称和模型ID选择合适的模型配置
        /// </summary>
        /// <param name="configuration">应用配置</param>
        /// <param name="provider">提供商名称，默认为AzureOpenAI</param>
        /// <param name="modelId">模型ID，如果为null则使用配置中默认的ModelName</param>
        /// <returns>配置好的Kernel</returns>
        public static Kernel CreateKernel(IConfiguration configuration, string? provider = "AzureOpenAI", string? modelId = null)
        {
            var builder = Kernel.CreateBuilder();

            // 从providers列表中查找指定的provider
            var providers = configuration.GetSection("AIProviders").Get<List<AIProvider>>() ?? new List<AIProvider>();
            var selectedProvider = providers.FirstOrDefault(p => p.Name == provider);

            if (selectedProvider == null)
            {
                throw new ArgumentException($"在配置中未找到名为 {provider} 的提供商", nameof(provider));
            }

            // 如果指定了modelId，则使用它覆盖默认的ModelId
            if (!string.IsNullOrEmpty(modelId))
            {
                selectedProvider.ModelId = modelId;
            }

            // 根据提供商名称选择合适的服务添加方式
            switch (selectedProvider.Name)
            {
                case "OpenAI":
                    builder.AddOpenAIChatCompletion(
                        selectedProvider.ModelId,
                        selectedProvider.ApiKey);
                    break;
                default:
                    throw new ArgumentException($"不支持的AI提供商: {selectedProvider.Name}", nameof(provider));
            }


            return builder.Build();
        }

        /// <summary>
        /// 配置聊天选项
        /// </summary>
        /// <param name="request">请求信息</param>
        /// <returns>聊天选项</returns>
        public static OpenAIPromptExecutionSettings CreateChatRequestSettings(AskRequest request)
        {
            return new OpenAIPromptExecutionSettings
            {
                MaxTokens = request.MaxTokens,
                Temperature = request.Temperature
            };
        }

        /// <summary>
        /// 创建带有系统提示的聊天历史
        /// </summary>
        /// <param name="userPrompt">用户提示</param>
        /// <param name="promptTemplate">提示模板类型</param>
        /// <returns>聊天历史</returns>
        public static ChatHistory CreateChatHistory(string userPrompt, PromptEnum promptTemplate)
        {
            var history = new ChatHistory();

            // 添加系统提示
            var systemPrompt = promptTemplate.GetPrompt();
            if (!string.IsNullOrEmpty(systemPrompt))
            {
                history.AddSystemMessage(systemPrompt);
            }

            // 添加用户消息
            history.AddUserMessage(userPrompt);

            return history;
        }
    }
}
