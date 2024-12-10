namespace DataflowBuilder;

public sealed class IntermediateBuildingBlock<TInitialIn, TIn>
{
  private Pipeline<TInitialIn> Pipeline { get; }

  internal IntermediateBuildingBlock(Pipeline<TInitialIn> pipeline)
    => Pipeline = pipeline;

  /// <summary>
  /// Add a block to the pipeline. This method is primarily for
  /// synchronous operation.
  /// </summary>
  /// <typeparam name="TOut">Output type that this block produces.</typeparam>
  /// <param name="func">The logic of this block.</param>
  /// <param name="pipelineBlockOptions">Pipeline block options.</param>
  /// <param name="allowTaskOutput">
  /// If true, this allows <see cref="Task"/> to be the output type.
  /// Else, disallow <see cref="Task"/> to be the output type and throw
  /// an exception if <see cref="Task"/> is specified as the output type.
  /// To return <see cref="Task"/>, please use the method
  /// <see cref="AddAsyncBlock{TOut}(Func{TIn, Task{TOut}}, PipelineBlockOptions?)"/>
  /// instead.
  /// </param>
  public IntermediateBuildingBlock<TInitialIn, TOut> AddBlock<TOut>(
    Func<TIn, TOut> func,
    PipelineBlockOptions? pipelineBlockOptions = null,
    bool allowTaskOutput = false
  )
  {
    Pipeline.AddBlock(func, pipelineBlockOptions, allowTaskOutput);
    return new(Pipeline);
  }

  /// <summary>
  /// Add a block to the pipeline. This method is primarily for
  /// asynchronous operation. Normally when a block produces a <see cref="Task"/>
  /// object, the next block's input type must be <see cref="Task"/> as well. But
  /// this method removes "<see cref="Task"/>" and instead it "returns" the type
  /// inside <see cref="Task"/>. 
  /// </summary>
  /// <typeparam name="TOut">Output type that this block produces.</typeparam>
  /// <param name="func">The async logic of this block.</param>
  /// <param name="pipelineBlockOptions">Pipeline block options.</param>
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

  public IntermediateBroadcastBlock<TInitialIn, TIn> Broadcast(DataflowLinkOptions? linkOptions = null)
  {
    Pipeline.Broadcast<TIn>(linkOptions);
    return new(Pipeline);
  }
}
