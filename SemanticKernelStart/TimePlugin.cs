namespace SemanticKernelStart;

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins;

public class TimePlugin
{
    [KernelFunction("now")]
    public string GetNow() => DateTime.Now.ToString();
}