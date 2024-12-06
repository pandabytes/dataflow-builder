namespace DataflowBuilder;

public sealed class IntermediateForkBlock<TInitialIn, TIn>
{
  private Pipeline<TInitialIn> Pipeline { get; }

  internal IntermediateForkBlock(Pipeline<TInitialIn> pipeline)
    => Pipeline = pipeline;

  public IntermediateForkBlock<TInitialIn, TIn> Branch(
    Predicate<TIn> predicate,
    Pipeline<TIn> branchPipeline,
    DataflowLinkOptions? linkOptions = null
  )
  {
    Pipeline.Branch(predicate, branchPipeline, linkOptions);
    return this;
  }

  public IntermediateForkBlock<TInitialIn, TIn> Default(
    Pipeline<TIn> branchPipeline,
    DataflowLinkOptions? linkOptions = null
  )
  {
    Pipeline.Default(branchPipeline, linkOptions);
    return this;
  }
}