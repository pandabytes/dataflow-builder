namespace DataflowBuilder;

/// <summary>
/// Intermediate block for building the 
/// fork functionality in the pipeline.
/// </summary>
/// <typeparam name="TPipelineFirstIn">Intial type that pipeline accepts.</typeparam>
/// <typeparam name="TIn">Input type for forking.</typeparam>
public sealed class IntermediateForkBlock<TPipelineFirstIn, TIn>
{
  private Pipeline<TPipelineFirstIn> Pipeline { get; }

  internal IntermediateForkBlock(Pipeline<TPipelineFirstIn> pipeline)
    => Pipeline = pipeline;

  /// <summary>
  /// Connect the current pipeline to <paramref name="branchPipeline"/>
  /// based on the <paramref name="predicate"/>. Note if <see cref="Default(Pipeline{TIn}, DataflowLinkOptions?)"/>
  /// is not specified, your pipeline may be stuck because there may values
  /// that do not satisfy the <paramref name="predicate"/> and they have
  /// no place to go. This is by design in TPL Dataflow.
  /// </summary>
  /// <param name="predicate">
  /// Predicate determining when value should go to <paramref name="branchPipeline"/>
  /// </param>
  /// <param name="branchPipeline">Branch pipeline to connect to.</param>
  /// <param name="linkOptions">Link options.</param>
  /// <returns></returns>
  public IntermediateForkBlock<TPipelineFirstIn, TIn> Branch(
    Predicate<TIn> predicate,
    Pipeline<TIn> branchPipeline,
    DataflowLinkOptions? linkOptions = null
  )
  {
    Pipeline.BranchOrDefault(branchPipeline, predicate, linkOptions);
    return this;
  }

  /// <summary>
  /// Connect the current pipeline to <paramref name="branchPipeline"/>
  /// without any predicate. This means by default, any value that does not satisfy
  /// a certain predicate will be delivered to this <paramref name="branchPipeline"/>.
  /// </summary>
  /// <param name="branchPipeline">Branch pipeline to connect to.</param>
  /// <param name="linkOptions">Link options.</param>
  public IntermediateForkBlock<TPipelineFirstIn, TIn> Default(
    Pipeline<TIn> branchPipeline,
    DataflowLinkOptions? linkOptions = null
  )
  {
    Pipeline.BranchOrDefault(branchPipeline, linkOptions: linkOptions);
    return this;
  }
}
