namespace DataflowBuilder;

public class IntermediateFork<TInitialIn, TIn>
{
  protected PipelineBuilder<TInitialIn> Builder { get; }

  internal IntermediateFork(PipelineBuilder<TInitialIn> pipelineBuilder)
    => Builder = pipelineBuilder;

  public IntermediateFork<TInitialIn, TIn> Branch(
    Predicate<TIn> predicate,
    PipelineBuilder<TIn> branchPipelineBuilder,
    DataflowLinkOptions? linkOptions = null
  )
  {
    Builder.Branch(predicate, branchPipelineBuilder, linkOptions);
    return this;
  }

  public IntermediateFork<TInitialIn, TIn> Default(
    PipelineBuilder<TIn> branchPipelineBuilder,
    DataflowLinkOptions? linkOptions = null
  )
  {
    Builder.Default(branchPipelineBuilder, linkOptions);
    return this;
  }
}
