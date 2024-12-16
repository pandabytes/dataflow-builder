namespace DataflowBuilder.Runners;

/// <summary>
/// Runner for excuting the pipeline with input values.
/// </summary>
/// <typeparam name="TIn">Type of input values.</typeparam>
public sealed class PipelineRunner<TIn> : BasePipelineRunner<TIn>
{
  /// <summary>
  /// First block of the pipeline.
  /// </summary>
  private readonly ITargetBlock<TIn> _firstBlock;

  /// <summary>
  /// Last blocks in the pipeline. Plural because a
  /// pipeline may have branched off into multiple branches,
  /// so it will have multiple "terminating" blocks.
  /// </summary>
  private readonly IEnumerable<IDataflowBlock> _lastBlocks;

  private bool _pipelineRanOrRunning;

  internal PipelineRunner(ITargetBlock<TIn> firstBlock, IEnumerable<IDataflowBlock> lastBlocks)
  {
    _firstBlock = firstBlock;
    _lastBlocks = lastBlocks;
    _pipelineRanOrRunning = false;
  }

  /// <inheritdoc/>
  public override async Task ExecuteAsync(IEnumerable<TIn> inputs, CancellationToken cancellationToken = default)
  {
    if (_pipelineRanOrRunning)
    {
      throw new InvalidOperationException("Pipeline is running or already ran.");
    }

    _pipelineRanOrRunning = true;

    foreach (var input in inputs)
    {
      await _firstBlock.SendAsync(input, cancellationToken).NoState();
    }

    _firstBlock.Complete();

    var completionTasks = _lastBlocks.Select(block => block.Completion);
    await Task.WhenAll(completionTasks).NoState();
  }

  /// <inheritdoc/>
  public override async Task ExecuteAsync(IAsyncEnumerable<TIn> inputs, CancellationToken cancellationToken = default)
  {
    if (_pipelineRanOrRunning)
    {
      throw new InvalidOperationException("Pipeline is running or already ran.");
    }

    _pipelineRanOrRunning = true;

    await foreach (var input in inputs)
    {
      await _firstBlock.SendAsync(input, cancellationToken).NoState();
    }

    _firstBlock.Complete();

    var completionTasks = _lastBlocks.Select(block => block.Completion);
    await Task.WhenAll(completionTasks).NoState();
  }
}