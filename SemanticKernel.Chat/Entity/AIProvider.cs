namespace SemanticKernel.Chat.Entity
{
    public class AIProvider
    {
        /// <summary>
        /// 提供商名称
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// 模型ID
        /// </summary>
        public string ModelId { get; set; } = string.Empty;
        
        /// <summary>
        /// API密钥
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;
        
        /// <summary>
        /// API端点
        /// </summary>
        public string? Endpoint { get; set; } = null;
    }
} 