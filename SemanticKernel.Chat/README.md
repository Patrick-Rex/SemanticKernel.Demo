# SemanticKernel 聊天 API 演示

这是一个基于 Microsoft Semantic Kernel 的大模型调用 API 演示项目，提供了类似 Cursor 或 DeepSeek 的提问接口能力。

## 功能特点

- 支持多种 AI 模型提供商配置（目前实现了 AzureOpenAI 和 OpenAI）
- 支持多种提示词模板选择
- 支持流式响应（SSE）和一次性响应
- 符合 OpenAI API 标准格式的响应
- 可配置的温度和最大令牌数

## API 接口

### 1. `/api/chat` - 基础聊天接口

简单的聊天接口，一次性返回完整响应。

**请求示例：**

```http
POST /api/chat
Content-Type: application/json

"你好，你能告诉我什么是Semantic Kernel吗？"
```

### 2. `/api/ask` - 高级聊天接口（类似 Cursor/DeepSeek）

支持模型选择、提示词模板和流式输出的高级接口。

**请求示例：**

```http
POST /api/ask
Content-Type: application/json

{
  "prompt": "你好，请用简单的话解释什么是Semantic Kernel？",
  "provider": "AzureOpenAI",
  "modelId": "gpt-35-turbo",
  "promptTemplate": "TechSupport_Chat_Prompt",
  "enableStreaming": true,
  "temperature": 0.7,
  "maxTokens": 2000
}
```

## 配置

在 `appsettings.json` 中配置各种 AI 模型：

```json
{
  "AIModel": {
    "DeploymentName": "gpt-35-turbo",
    "ModelName": "gpt-35-turbo",
    "Endpoint": "https://your-azure-openai-endpoint.openai.azure.com/",
    "ApiKey": "your-azure-openai-api-key",
    "Provider": "AzureOpenAI"
  },
  "AIProviders": [
    {
      "Name": "OpenAI",
      "ModelId": "gpt-3.5-turbo",
      "ApiKey": "your-openai-key",
      "Endpoint": null
    },
    {
      "Name": "AnotherAIProvider",
      "ModelId": "model-id",
      "ApiKey": "your-api-key",
      "Endpoint": "your-endpoint"
    }
  ]
}
```

## 提示词模板

系统内置了多种提示词模板：

- `Contoso_Chat_Prompt` - 标准聊天提示词
- `TechSupport_Chat_Prompt` - 技术支持提示词
- `CustomerService_Chat_Prompt` - 客户服务提示词
- `React_Chat_Prompt` - React 开发专家提示词

## 开发扩展

如需添加新的 AI 提供商支持，请修改 `AIModelClientHandler.cs` 中的 `CreateKernel` 方法。