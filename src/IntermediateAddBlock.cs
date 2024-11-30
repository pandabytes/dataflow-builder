namespace DataflowBuilder;

public class IntermediateAddBlock<TIn>
{
  protected PipelineBuilder Builder { get; }

  internal IntermediateAddBlock(PipelineBuilder pipelineBuilder)
  {
    Builder = pipelineBuilder;
  }

  public IntermediateAddBlock<TOut> AddBlock<TOut>(Func<TIn, TOut> func, PipelineBlockOptions? pipelineBlockOptions = null)
  {
    Builder.AddBlock(func, pipelineBlockOptions);
    return new(Builder);
  }

  public IntermediateAddBlock<TOut> AddAsyncBlock<TOut>(Func<TIn, Task<TOut>> func, PipelineBlockOptions? pipelineBlockOptions = null)
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
