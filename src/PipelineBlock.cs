namespace DataflowBuilder;

/// <summary>
/// Represent a block in the pipeline.
/// </summary>
public class PipelineBlock
{
  /// <summary>
  /// The underlying Dataflow block.
  /// </summary>
  public required IDataflowBlock Value { get; init; }

  /// <summary>
  /// Indicate whether this block returns an async
  /// value, i.e <see cref="Task"/> object.
  /// </summary>
  public required bool IsBlockAsync { get; init; }
}
