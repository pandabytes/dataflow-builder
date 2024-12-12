namespace DataflowBuilder;

/// <summary>
/// This class serves as the "intermediate" building block
/// for building a pipeline. It ensures the property input and output
/// type are consistent and correct when building a pipeline.
/// </summary>
/// <typeparam name="TPipelineFirstIn">Intial type that pipeline accepts.</typeparam>
/// <typeparam name="TIn">Input type for the next building block.</typeparam>
public sealed class IntermediateBuildingBlock<TPipelineFirstIn, TIn>
{
  private Pipeline<TPipelineFirstIn> Pipeline { get; }

  internal IntermediateBuildingBlock(Pipeline<TPipelineFirstIn> pipeline)
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
  public IntermediateBuildingBlock<TPipelineFirstIn, TOut> AddBlock<TOut>(
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
  public IntermediateBuildingBlock<TPipelineFirstIn, TOut> AddAsyncBlock<TOut>(
    Func<TIn, Task<TOut>> func, 
    PipelineBlockOptions? pipelineBlockOptions = null
  )
  {
    Pipeline.AddAsyncBlock(func, pipelineBlockOptions);
    return new(Pipeline);
  }

  /// <summary>
  /// Add a block to the pipeline where the output is delivered to the
  /// next block as if it's a single element until the <see cref="IEnumerable{T}"/>.
  /// is fully enumerated.
  /// every element in
  /// </summary>
  /// <typeparam name="TOut">Output type that this block produces.</typeparam>
  /// <param name="func">The async logic of this block.</param>
  /// <param name="pipelineBlockOptions">Pipeline block options.</param>
  public IntermediateBuildingBlock<TPipelineFirstIn, TOut> AddManyBlock<TOut>(
    Func<TIn, IEnumerable<TOut>> func,
    PipelineBlockOptions? pipelineBlockOptions = null
  )
  {
    Pipeline.AddManyBlock(func, pipelineBlockOptions);
    return new(Pipeline);
  }

  /// <summary>
  /// Add the last synchronous block to the pipeline, indicating the pipeline
  /// is ready to be built.
  /// </summary>
  /// <param name="action">The logic of this block.</param>
  /// <param name="pipelineBlockOptions">Pipeline block options.</param>
  public void AddLastBlock(Action<TIn> action, PipelineBlockOptions? pipelineBlockOptions = null)
  {
    Pipeline.AddLastBlock(action, pipelineBlockOptions);
  }

  /// <summary>
  /// Add the last asynchronous block to the pipeline, indicating the pipeline
  /// is ready to be built.
  /// </summary>
  /// <param name="func">The logic of this block.</param>
  /// <param name="pipelineBlockOptions">Pipeline block options.</param>
  public void AddLastAsyncBlock(Func<TIn, Task> func, PipelineBlockOptions? pipelineBlockOptions = null)
  {
    Pipeline.AddLastAsyncBlock(func, pipelineBlockOptions);
  }

  /// <summary>
  /// Fork a pipeline into multiple branch pipelines.
  /// </summary>
  /// <returns>An object that allows connecting to branch pipelines.</returns>
  public IntermediateForkBlock<TPipelineFirstIn, TIn> Fork()
  {
    Pipeline.Fork();
    return new(Pipeline);
  }

  /// <summary>
  /// Broadcast the value from the last block in the current pipeline
  /// to multiple branch pipelines.
  /// </summary>
  /// <param name="cloningFunc">The function to use to clone the data when offered to other blocks.</param>
  /// <param name="pipelineBlockOptions">Pipeline block options.</param>
  /// <returns>An object that allows connecting to branch pipelines.</returns>
  public IntermediateBroadcastBlock<TPipelineFirstIn, TIn> Broadcast(Func<TIn, TIn>? cloningFunc = null, PipelineBlockOptions? pipelineBlockOptions = null)
  {
    Pipeline.Broadcast(cloningFunc, pipelineBlockOptions);
    return new(Pipeline);
  }
}
