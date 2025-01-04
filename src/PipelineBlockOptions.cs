namespace DataflowBuilder;

/// <summary>
/// Options for a block in a pipeline.
/// </summary>
public class PipelineBlockOptions<TBlock> where TBlock : DataflowBlockOptions
{
  /// <summary>
  /// Options for the block itself.
  /// </summary>
  public TBlock? BlockOptions { get; init; }

  /// <summary>
  /// Options for link between 2 blocks.
  /// </summary>
  public DataflowLinkOptions? LinkOptions { get; init; }
}
