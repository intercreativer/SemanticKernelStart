using Microsoft.SemanticKernel;
using SemanticKernelStart;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<Kernel>(sp =>
{
    var kernelBuilder = Kernel.CreateBuilder()
        .AddOpenAIChatCompletion(
            modelId: "phi3",
            apiKey: "",
            endpoint: new Uri("http://localhost:11434/v1/")  // Ollama default
            );
    
    kernelBuilder.Plugins.AddFromType<TimePlugin>();
    
    
    return kernelBuilder.Build();
});


var app = builder.Build();

app.MapGet("/ask", async (Kernel kernel, string q) =>
{
    try
    {
        var result = await kernel.InvokePromptAsync(q);
        return Results.Ok(result.ToString());
    }
    catch (Exception ex)
    {
        // Check for quota-related errors
        if (ex.Message.Contains("429") || 
            ex.Message.Contains("insufficient_quota") || 
            ex.Message.Contains("quota"))
        {
            return Results.Problem(
                title: "Quota Exceeded",
                detail: "Your OpenAI account has exceeded its quota or has insufficient credits. Please check your billing details at https://platform.openai.com/account/billing",
                statusCode: 429
            );
        }
        
        return Results.Problem(
            title: "Error processing request",
            detail: ex.Message,
            statusCode: 500
        );
    }
});

app.MapGet("/time", async (Kernel kernel) =>
{
    try
    {
        var result = await kernel.InvokePromptAsync("""
            What time is it? Use the function if needed.
            """);
        return Results.Ok(result.ToString());
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: "Error processing request",
            detail: ex.Message,
            statusCode: 500
        );
    }
});

app.Run();