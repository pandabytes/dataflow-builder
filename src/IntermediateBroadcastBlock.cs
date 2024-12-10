namespace DataflowBuilder;

public sealed class IntermediateBroadcastBlock<TPipelineFirstIn, TIn>
{
  private Pipeline<TPipelineFirstIn> Pipeline { get; }

  internal IntermediateBroadcastBlock(Pipeline<TPipelineFirstIn> pipeline)
    => Pipeline = pipeline;

  public IntermediateBroadcastBlock<TPipelineFirstIn, TIn> Branch(
    Pipeline<TIn> branchPipeline,
    DataflowLinkOptions? linkOptions = null
  )
  {
    Pipeline.BranchOrDefault(branchPipeline, null, linkOptions);
    return this;
  }
}