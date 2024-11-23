namespace PipelineBuilder;

public class Pipeline<TIn>
{
  private readonly ITargetBlock<TIn> _firstBlock;

  private readonly IEnumerable<IDataflowBlock> _lastBlocks;

  private bool _pipelineRan;

  internal Pipeline(ITargetBlock<TIn> firstBlock, IEnumerable<IDataflowBlock> lastBlocks)
  {
    _firstBlock = firstBlock;
    _lastBlocks = lastBlocks;
    _pipelineRan = false;
  }

  public async Task ExecuteAsync(IEnumerable<TIn> inputs, CancellationToken cancellationToken = default)
  {
    if (_pipelineRan)
    {
      throw new InvalidOperationException("Pipeline already ran. Cannot rerun it.");
    }

    try
    {
      foreach (var input in inputs)
      {
        await _firstBlock.SendAsync(input, cancellationToken).NoState();
      }

      _firstBlock.Complete();

      var completionTasks = _lastBlocks.Select(block => block.Completion);
      await Task.WhenAll(completionTasks).NoState();
    }
    finally
    {
      _pipelineRan = true;
    }
  }
}