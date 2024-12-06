namespace DataflowBuilder;

public sealed class IntermediateBuildingBlock<TInitialIn, TIn>
{
  private Pipeline<TInitialIn> Pipeline { get; }

  internal IntermediateBuildingBlock(Pipeline<TInitialIn> pipeline)
    => Pipeline = pipeline;

  public IntermediateBuildingBlock<TInitialIn, TOut> AddBlock<TOut>(
    Func<TIn, TOut> func,
    PipelineBlockOptions? pipelineBlockOptions = null
  )
  {
    Pipeline.AddBlock(func, pipelineBlockOptions);
    return new(Pipeline);
  }

  public IntermediateBuildingBlock<TInitialIn, TOut> AddAsyncBlock<TOut>(
    Func<TIn, Task<TOut>> func, 
    PipelineBlockOptions? pipelineBlockOptions = null
  )
  {
    Pipeline.AddAsyncBlock(func, pipelineBlockOptions);
    return new(Pipeline);
  }

  public void AddLastBlock(Action<TIn> action, PipelineBlockOptions? pipelineBlockOptions = null)
  {
    Pipeline.AddLastBlock(action, pipelineBlockOptions);
  }

  public void AddLastAsyncBlock(Func<TIn, Task> func, PipelineBlockOptions? pipelineBlockOptions = null)
  {
    Pipeline.AddLastAsyncBlock(func, pipelineBlockOptions);
  }

  public IntermediateForkBlock<TInitialIn, TIn> Fork()
  {
    Pipeline.Fork();
    return new(Pipeline);
  }
}
