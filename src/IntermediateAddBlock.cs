namespace PipelineBuilder;

public class IntermediateAddBlock<TIn>
{
  protected DataflowPipelineBuilder Builder { get; }

  internal IntermediateAddBlock(DataflowPipelineBuilder pipelineBuilder)
  {
    Builder = pipelineBuilder;
  }

  public IntermediateAddBlock<TOut> AddBlock<TOut>(Func<TIn, TOut> func, ExecutionDataflowBlockOptions? blockOptions = null, DataflowLinkOptions? linkOptions = null)
  {
    Builder.AddBlock(func, blockOptions, linkOptions);
    return new(Builder);
  }

  public IntermediateAddBlock<TOut> AddAsyncBlock<TOut>(Func<TIn, Task<TOut>> func, ExecutionDataflowBlockOptions? blockOptions = null, DataflowLinkOptions? linkOptions = null)
  {
    Builder.AddAsyncBlock(func, blockOptions, linkOptions);
    return new(Builder);
  }

  public void AddLastBlock(Action<TIn> action, ExecutionDataflowBlockOptions? blockOptions = null, DataflowLinkOptions? linkOptions = null)
  {
    Builder.AddLastBlock(action, blockOptions, linkOptions);
  }

  public void AddLastAsyncBlock(Func<TIn, Task> func, ExecutionDataflowBlockOptions? blockOptions = null, DataflowLinkOptions? linkOptions = null)
  {
    Builder.AddLastAsyncBlock(func, blockOptions, linkOptions);
  }
}
