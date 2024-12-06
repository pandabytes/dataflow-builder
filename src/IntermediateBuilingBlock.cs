namespace DataflowBuilder;

public class IntermediateBuilingBlock<TInitialIn, TIn>
{
  protected PipelineBuilder<TInitialIn> Builder { get; }

  internal IntermediateBuilingBlock(PipelineBuilder<TInitialIn> pipelineBuilder)
    => Builder = pipelineBuilder;

  public IntermediateBuilingBlock<TInitialIn, TOut> AddBlock<TOut>(Func<TIn, TOut> func, PipelineBlockOptions? pipelineBlockOptions = null)
  {
    Builder.AddBlock(func, pipelineBlockOptions);
    return new(Builder);
  }

  public IntermediateBuilingBlock<TInitialIn, TOut> AddAsyncBlock<TOut>(Func<TIn, Task<TOut>> func, PipelineBlockOptions? pipelineBlockOptions = null)
  {
    Builder.AddAsyncBlock(func, pipelineBlockOptions);
    return new(Builder);
  }

  public void AddLastBlock(Action<TIn> action, PipelineBlockOptions? pipelineBlockOptions = null)
  {
    Builder.AddLastBlock(action, pipelineBlockOptions);
  }

  public void AddLastAsyncBlock(Func<TIn, Task> func, PipelineBlockOptions? pipelineBlockOptions = null)
  {
    Builder.AddLastAsyncBlock(func, pipelineBlockOptions);
  }

  public IntermediateFork<TInitialIn, TIn> Fork()
  {
    Builder.Fork();
    return new(Builder);
  }
}
