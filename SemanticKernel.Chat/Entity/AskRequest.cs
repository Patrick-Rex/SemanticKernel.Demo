using SemanticKernel.Chat.Enum;

namespace SemanticKernel.Chat.Entity
{
    public class AskRequest
    {
        /// <summary>
        /// 用户输入的提示内容
        /// </summary>
        public string Prompt { get; set; } = string.Empty;
        
        /// <summary>
        /// 提供商名称，默认为AzureOpenAI
        /// </summary>
        public string Provider { get; set; } = "AzureOpenAI";
        
        /// <summary>
        /// 模型ID，如果不提供则使用配置中的默认模型
        /// </summary>
        public string? ModelId { get; set; } = null;
        
        /// <summary>
        /// 提示词模板，用于设置系统消息
        /// </summary>
        public PromptEnum PromptTemplate { get; set; } = PromptEnum.Contoso_Chat_Prompt;
        
        /// <summary>
        /// 是否启用流式输出
        /// </summary>
        public bool EnableStreaming { get; set; } = true;
        
        /// <summary>
        /// 温度参数，控制输出的随机性
        /// </summary>
        public float Temperature { get; set; } = 0.7f;
        
        /// <summary>
        /// 最大输出令牌数
        /// </summary>
        public int MaxTokens { get; set; } = 2000;
    }
}
