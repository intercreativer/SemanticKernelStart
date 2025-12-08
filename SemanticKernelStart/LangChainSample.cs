#if LANGCHAIN_SAMPLE
using LangChain.Providers;
using LangChain.Providers.OpenAI;
using LangChain.Providers.OpenAI.Predefined;

namespace SemanticKernelStart;

/// <summary>
/// Simple LangChain.NET wrapper that reuses the OpenAI credentials from configuration.
/// </summary>
public class LangChainSample
{
    private readonly OpenAiChatModel _chatModel;
    private readonly OpenAiChatSettings _chatSettings;

    public LangChainSample(IConfiguration configuration)
    {
        var apiKey = configuration["OpenAI:ApiKey"]
                     ?? throw new InvalidOperationException("OpenAI:ApiKey is required for LangChain.");

        var provider = new OpenAiProvider(apiKey);
        var model = configuration["OpenAI:Model"];
        _chatModel = string.IsNullOrWhiteSpace(model)
            ? new OpenAiLatestFastChatModel(provider)
            : new OpenAiChatModel(provider, model);

        _chatSettings = new OpenAiChatSettings
        {
            Temperature = 0.2f
        };
    }

    public async Task<string> AskAsync(string question, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(question))
        {
            throw new ArgumentException("Question cannot be empty.", nameof(question));
        }

        var request = ChatRequest.ToChatRequest(new[]
        {
            new Message("You are a concise travel assistant. Keep answers short and friendly.", MessageRole.System, string.Empty),
            Message.Human(question)
        });

        var response = await _chatModel.GenerateAsync(request, _chatSettings, cancellationToken);
        return response.LastMessageContent?.Trim() ?? string.Empty;
    }
}
#endif
