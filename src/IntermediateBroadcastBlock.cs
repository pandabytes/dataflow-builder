namespace DataflowBuilder;

public sealed class IntermediateBroadcastBlock<TInitialIn, TIn>
{
  private Pipeline<TInitialIn> Pipeline { get; }

  internal IntermediateBroadcastBlock(Pipeline<TInitialIn> pipeline)
    => Pipeline = pipeline;

  public IntermediateBroadcastBlock<TInitialIn, TIn> Branch(
    Pipeline<TIn> branchPipeline,
    DataflowLinkOptions? linkOptions = null
  )
  {
    Pipeline.BranchOrDefault(branchPipeline, null, linkOptions);
    return this;
  }
}