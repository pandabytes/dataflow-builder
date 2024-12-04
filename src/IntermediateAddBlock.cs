namespace DataflowBuilder;

public class IntermediateAddBlock<TInitialIn, TIn>
{
  protected PipelineBuilder<TInitialIn> Builder { get; }

  internal IntermediateAddBlock(PipelineBuilder<TInitialIn> pipelineBuilder)
  {
    Builder = pipelineBuilder;
  }

  public IntermediateAddBlock<TInitialIn, TOut> AddBlock<TOut>(Func<TIn, TOut> func, PipelineBlockOptions? pipelineBlockOptions = null)
  {
    Builder.AddBlock(func, pipelineBlockOptions);
    return new(Builder);
  }

  public IntermediateAddBlock<TInitialIn, TOut> AddAsyncBlock<TOut>(Func<TIn, Task<TOut>> func, PipelineBlockOptions? pipelineBlockOptions = null)
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
}
