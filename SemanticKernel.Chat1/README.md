

根目录下添加如下格式的配置文件 `appsettings.json`，并填入你的 OpenAI 和 Gemini API Key。
```json
{
  "OpenAI": {
    "ModelId": "gpt-3.5
{
  "OpenAI": {
    "ModelId": "gpt-3.5-turbo",
    "ApiKey": "your-openai-key",
    "Endpoint": null
  },
  "Gemini": {
    "ModelId": "gemini-pro",
    "ApiKey": "your-gemini-key",
    "Endpoint": "https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent"
  }
}