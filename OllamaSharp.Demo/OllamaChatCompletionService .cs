using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using OllamaSharp;
using OllamaSharp.Models.Chat;


namespace OllamaSharp.Demo
{
    public class OllamaChatCompletionService : IChatCompletionService
    {
        // public property for the model url endpoint
        public string ModelUrl { get; set; }
        public string ModelName { get; set; }

        public IReadOnlyDictionary<string, object?> Attributes => throw new NotImplementedException();

        public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(ChatHistory chatHistory, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, CancellationToken cancellationToken = default)
        {
            var ollama = new OllamaApiClient(ModelUrl, ModelName); // (uri);

            Chat chat = new Chat(ollama, string.Empty);


            // iterate though chatHistory Messages
            foreach (var message in chatHistory)
            {
                if (message.Role == AuthorRole.System)
                {
                    chat.SendAsAsync(ChatRole.System, message.Content);
                    continue;
                }
            }

            var lastMessage = chatHistory.LastOrDefault();

            var question = lastMessage.Content;
            var chatResponse = string.Empty;
            var history = chat.SendAsync(question, CancellationToken.None);

            chatResponse = await history.LastOrDefaultAsync();

            chatHistory.AddAssistantMessage(chatResponse);

            return chatHistory;
        }

        public IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(ChatHistory chatHistory, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

    }
}
