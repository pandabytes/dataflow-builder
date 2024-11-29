namespace DataflowBuilder;

public class Pipeline<TIn>
{
  private readonly ITargetBlock<TIn> _firstBlock;

  private readonly IEnumerable<IDataflowBlock> _lastBlocks;

  private bool _pipelineRanOrRunning;

  internal Pipeline(ITargetBlock<TIn> firstBlock, IEnumerable<IDataflowBlock> lastBlocks)
  {
    _firstBlock = firstBlock;
    _lastBlocks = lastBlocks;
    _pipelineRanOrRunning = false;
  }

  public async Task ExecuteAsync(IEnumerable<TIn> inputs, CancellationToken cancellationToken = default)
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

  public async Task ExecuteAsync(IAsyncEnumerable<TIn> inputs, CancellationToken cancellationToken = default)
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