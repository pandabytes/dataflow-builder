using System.Runtime.CompilerServices;

namespace DataflowBuilder.Extensions;

internal static class TaskExtensions
{
  public static ConfiguredTaskAwaitable NoState(this Task task) => task.ConfigureAwait(false);
}
