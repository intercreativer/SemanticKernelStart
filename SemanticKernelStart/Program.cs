using Microsoft.SemanticKernel;
using SemanticKernelStart;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<Kernel>(sp =>
{
    var configuration = builder.Configuration;

    var modelId = configuration["OpenAI:Model"];
    var apiKey = configuration["OpenAI:ApiKey"];

    var kernelBuilder = Kernel.CreateBuilder()
        .AddOpenAIChatCompletion(
            modelId: modelId,
            apiKey: apiKey
        );
    
    kernelBuilder.Plugins.AddFromType<TimePlugin>();
    
    
    return kernelBuilder.Build();
});


var app = builder.Build();

app.MapGet("/ask", async (Kernel kernel, string q) =>
{
    try
    {
        // Enable auto function calling
        var executionSettings = new PromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };
        
        var result = await kernel.InvokePromptAsync(q, new KernelArguments(executionSettings));
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
        // Enable auto function calling
        var prompt = "What time is it? Use the function if needed.";
        var executionSettings = new PromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };
        
        var result = await kernel.InvokePromptAsync(prompt, new KernelArguments(executionSettings));
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
