# LangChain.NET Sample

This project ships with a `LangChainSample` class (`LangChainSample.cs`) that illustrates how to wrap the
[LangChain for .NET](https://www.nuget.org/packages/LangChain) library inside ASP.NET Core.

## How to run it

1. Install the packages from your terminal or IDE:

   ```bash
   dotnet add package LangChain
   dotnet add package LangChain.Providers.OpenAI
   ```

2. Define the `LANGCHAIN_SAMPLE` symbol so the example class compiles. You can do this in
   `SemanticKernelStart.csproj`:

   ```xml
   <PropertyGroup>
     <DefineConstants>$(DefineConstants);LANGCHAIN_SAMPLE</DefineConstants>
   </PropertyGroup>
   ```

3. Inject `LangChainSample` where needed (e.g., add `builder.Services.AddSingleton<LangChainSample>();`)
   and call `AskAsync("Plan a short Seattle walking tour")` to see the chain in action.

The sample uses the same `OpenAI:ApiKey` and `OpenAI:Model` settings already present in `appsettings.json`,
builds an `LLMChain` with a `PromptTemplate`, and returns the generated text.
