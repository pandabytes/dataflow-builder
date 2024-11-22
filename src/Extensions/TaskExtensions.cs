using System.Runtime.CompilerServices;

namespace PipelineBuilder.Extensions;

internal static class TaskExtensions
{
  public static ConfiguredTaskAwaitable NoState(this Task task) => task.ConfigureAwait(false);
}
