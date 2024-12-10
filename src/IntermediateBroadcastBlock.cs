namespace DataflowBuilder;

/// <summary>
/// Intermediate block for building the 
/// broadcast functionality in the pipeline.
/// </summary>
/// <typeparam name="TPipelineFirstIn">Intial type that pipeline accepts.</typeparam>
/// <typeparam name="TIn">Input type for broadcasting.</typeparam>
public sealed class IntermediateBroadcastBlock<TPipelineFirstIn, TIn>
{
  private Pipeline<TPipelineFirstIn> Pipeline { get; }

  internal IntermediateBroadcastBlock(Pipeline<TPipelineFirstIn> pipeline)
    => Pipeline = pipeline;

  /// <summary>
  /// Connect the broadcast block of the current pipeline to
  /// <paramref name="branchPipeline"/>.
  /// </summary>
  /// <param name="branchPipeline">Branch pipeline to connect to.</param>
  /// <param name="linkOptions">Link options.</param>
  public IntermediateBroadcastBlock<TPipelineFirstIn, TIn> Branch(
    Pipeline<TIn> branchPipeline,
    DataflowLinkOptions? linkOptions = null
  )
  {
    Pipeline.BranchOrDefault(branchPipeline, null, linkOptions);
    return this;
  }
}