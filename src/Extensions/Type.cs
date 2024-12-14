namespace DataflowBuilder.Extensions;

internal static class TypeExtensions
{
  public static bool IsAsync(this Type type)
  {
    return type == typeof(Task) || 
            (type.IsGenericType && 
            type.GetGenericTypeDefinition() == typeof(Task<>));
  }

  public static Type GetTaskResultType(this Type taskType)
  {
    if (!IsAsync(taskType))
    {
      throw new ArgumentException("Expect type to be Task.");
    }

    var genericTypes = taskType.GetGenericArguments();
    return genericTypes.First();
  }

  /// <summary>
  /// For debugging purposes.
  /// </summary>
  public static void ShowGenericTypes(this Type type)
  {
    foreach (var genericType in type.GetGenericArguments())
    {
      System.Console.WriteLine(genericType.FullName);
    }
  }
}
