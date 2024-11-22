namespace PipelineBuilder;

public class DataflowPipelineBuilder
{
  private readonly IList<IDataflowBlock> _blocks;

  private bool _lastBlockAdded;

  public DataflowPipelineBuilder()
  {
    _blocks = new List<IDataflowBlock>();
    _lastBlockAdded = false;
  }

  internal IntermediateAddBlock<TOut> AddBlock<TIn, TOut>(Func<TIn, TOut> func, ExecutionDataflowBlockOptions? blockOptions = null, DataflowLinkOptions? linkOptions = null)
  {
    var newBlock = new TransformBlock<TIn, TOut>(func, blockOptions ?? new());

    if (_blocks.Count > 0)
    {
      var lastTargetBlock = _blocks.Last() as ISourceBlock<TIn>
        ?? throw new ArgumentException("Cannot link block to last block in the pipeline due to output type mismatch");
      lastTargetBlock.LinkTo(newBlock, linkOptions ?? new());        
    }
    _blocks.Add(newBlock);

    return new(this);
  }

  internal void AddLastBlock<TIn>(Action<TIn> action, ExecutionDataflowBlockOptions? blockOptions = null, DataflowLinkOptions? linkOptions = null)
  {
    var newBlock = new ActionBlock<TIn>(action, blockOptions ?? new());

    if (_blocks.Count > 0)
    {
      var lastTargetBlock = _blocks.Last() as ISourceBlock<TIn>
        ?? throw new ArgumentException("Cannot link block to last block in the pipeline due to output type mismatch");
      lastTargetBlock.LinkTo(newBlock, linkOptions ?? new());        
    }
    _blocks.Add(newBlock);
    _lastBlockAdded = true;
  }

  public IntermediateAddBlock<TOut> AddFirstBlock<TIn, TOut>(Func<TIn, TOut> func, ExecutionDataflowBlockOptions? blockOptions = null)
  {
    if (_blocks.Count > 0)
    {
      throw new InvalidOperationException("Pipeline must be empty when adding the first block.");
    }

    _blocks.Add(new TransformBlock<TIn, TOut>(func, blockOptions ?? new()));
    return new(this);
  }

  public Pipeline<TIn> Build<TIn>()
  {
    if (_blocks.Count == 0)
    {
      throw new InvalidOperationException("Pipeline does not have any block defined.");
    }

    if (!_lastBlockAdded)
    {
      throw new InvalidOperationException($"Must call {nameof(IntermediateAddBlock<TIn>.AddLastBlock)} " +
                                          "to indicate pipeline is ready to be built.");
    }

    var firstBlock = _blocks.First() as ITargetBlock<TIn>
      ?? throw new InvalidOperationException($"Input type of first block must match with type {typeof(TIn).FullName}.");
    IEnumerable<IDataflowBlock> lastBlocks = [_blocks.Last()];

    return new(firstBlock, lastBlocks);
  }
}
