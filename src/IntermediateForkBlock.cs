namespace DataflowBuilder;

public sealed class IntermediateForkBlock<TPipelineFirstIn, TIn>
{
  private Pipeline<TPipelineFirstIn> Pipeline { get; }

  internal IntermediateForkBlock(Pipeline<TPipelineFirstIn> pipeline)
    => Pipeline = pipeline;

  public IntermediateForkBlock<TPipelineFirstIn, TIn> Branch(
    Predicate<TIn> predicate,
    Pipeline<TIn> branchPipeline,
    DataflowLinkOptions? linkOptions = null
  )
  {
    Pipeline.BranchOrDefault(branchPipeline, predicate, linkOptions);
    return this;
  }

  public IntermediateForkBlock<TPipelineFirstIn, TIn> Default(
    Pipeline<TIn> branchPipeline,
    DataflowLinkOptions? linkOptions = null
  )
  {
    Pipeline.BranchOrDefault(branchPipeline, linkOptions: linkOptions);
    return this;
  }
}
