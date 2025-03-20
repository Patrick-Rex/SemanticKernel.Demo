using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Scalar.AspNetCore;
using SemanticKernel.Chat;
using SemanticKernel.Chat.Entity;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

IConfiguration config = new ConfigurationBuilder()
    .SetBasePath(Environment.CurrentDirectory)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();


app.MapPost("/api/chat", async ([FromBody] string input) =>
{
    try
    {
        // 创建默认的Kernel
        var kernel = AIModelClientHandler.CreateKernel(config);
        
        // 获取聊天完成服务
        var chat = kernel.GetRequiredService<IChatCompletionService>();
        
        // 创建聊天历史
        var history = new ChatHistory();
        history.AddUserMessage(input);

        // 获取模型响应
        var response = await chat.GetChatMessageContentsAsync(history);
        
        return Results.Ok(response[0].Content);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.MapPost("/api/ask", async (HttpContext context) =>
{
    try
    {
        // 读取请求体
        using var reader = new StreamReader(context.Request.Body);
        var body = await reader.ReadToEndAsync();
        var request = JsonSerializer.Deserialize<AskRequest>(body, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        });
        
        if (request == null)
        {
            return Results.BadRequest("无效的请求数据");
        }

        var provider = request.Provider;
        var modelId = request.ModelId;
        
        // 创建Kernel
        var kernel = AIModelClientHandler.CreateKernel(config, provider, modelId);

        // 获取聊天完成服务
        var chat = kernel.GetRequiredService<IChatCompletionService>();
        
        // 创建带有系统提示的聊天历史
        var history = AIModelClientHandler.CreateChatHistory(request.Prompt, request.PromptTemplate);
        
        // 创建聊天选项
        var settings = AIModelClientHandler.CreateChatRequestSettings(request);

        // 设置SSE响应头
        context.Response.Headers.Append("Content-Type", "text/event-stream");
        context.Response.Headers.Append("Cache-Control", "no-cache");
        context.Response.Headers.Append("Connection", "keep-alive");

        if (request.EnableStreaming)
        {
            // 流式输出
            var streamingOptions = new OpenAIPromptExecutionSettings 
            {
                MaxTokens = request.MaxTokens,
                Temperature = request.Temperature
            };
            
            await foreach (var message in chat.GetStreamingChatMessageContentsAsync(history, streamingOptions))
            {
                // 构造类似OpenAI的响应格式
                var content = message.Content;
                if (content != null)
                {
                    // 创建OpenAI格式的响应对象
                    var responseChunk = new 
                    {
                        id = $"chatcmpl-{Guid.NewGuid():N}",
                        model = request.ModelId ?? "default-model",
                        created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                        choices = new[] 
                        {
                            new 
                            {
                                delta = new { content },
                                index = 0,
                                finish_reason = string.Empty
                            }
                        }
                    };
                    
                    var json = JsonSerializer.Serialize(responseChunk);
                    await context.Response.WriteAsync($"data: {json}\n\n");
                    await context.Response.Body.FlushAsync();
                }
            }

            // 发送完成事件
            var completionEvent = new 
            {
                id = $"chatcmpl-{Guid.NewGuid():N}",
                model = request.ModelId ?? "default-model",
                created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                choices = new[] 
                {
                    new 
                    {
                        delta = new {},
                        index = 0,
                        finish_reason = "stop"
                    }
                }
            };
            
            var completionJson = JsonSerializer.Serialize(completionEvent);
            await context.Response.WriteAsync($"data: {completionJson}\n\n");
            await context.Response.WriteAsync("data: [DONE]\n\n");
        }
        else
        {
            // 一次性返回
            var response = await chat.GetChatMessageContentsAsync(history, settings);
            
            // 构造类似OpenAI的响应格式
            var responseObject = new 
            {
                id = $"chatcmpl-{Guid.NewGuid():N}",
                model = request.ModelId ?? "default-model",
                created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                choices = new[] 
                {
                    new 
                    {
                        message = new 
                        {
                            role = "assistant",
                            content = response[0].Content
                        },
                        index = 0,
                        finish_reason = "stop"
                    }
                }
            };
            
            var json = JsonSerializer.Serialize(responseObject);
            await context.Response.WriteAsync($"data: {json}\n\n");
        }
        
        await context.Response.Body.FlushAsync();
        return Results.Ok();
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        var errorResponse = new 
        {
            error = new 
            {
                message = ex.Message,
                type = "api_error",
                code = "internal_error"
            }
        };
        
        var errorJson = JsonSerializer.Serialize(errorResponse);
        await context.Response.WriteAsync($"data: {errorJson}\n\n");
        return Results.Ok();
    }
});

app.Run();
