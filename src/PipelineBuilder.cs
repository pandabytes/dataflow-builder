namespace DataflowBuilder;

public sealed class PipelineBuilder<TInitialIn> : IPipelineBuilder
{
  private readonly IList<PipelineBlock> _blocks;

  private bool _pipelineBuilt;

  private bool _lastBlockAdded;

  private readonly IList<IPipelineBuilder> _branchPipelineBuilders;

  PipelineBlock IPipelineBuilder.FirstBlock => _blocks.First();

  PipelineBlock IPipelineBuilder.LastBlock => _blocks.Last();

  IList<IPipelineBuilder> IPipelineBuilder.BranchPipelineBuilders => _branchPipelineBuilders;

  public PipelineBuilder()
  {
    _blocks = new List<PipelineBlock>();
    _branchPipelineBuilders = new List<IPipelineBuilder>();
    _lastBlockAdded = false;
    _pipelineBuilt = false;
  }

  public IntermediateBuilingBlock<TInitialIn, TOut> AddFirstBlock<TOut>(Func<TInitialIn, TOut> func, ExecutionDataflowBlockOptions? blockOptions = null)
  {
    if (_blocks.Count > 0)
    {
      throw new InvalidOperationException("Pipeline must be empty when adding the first block.");
    }

    if (IsAsync(typeof(TOut)))
    {
      throw new InvalidOperationException($"Please use the method {nameof(IntermediateBuilingBlock<TInitialIn, TOut>.AddAsyncBlock)} for async operation.");
    }

    var newBlock = new TransformBlock<TInitialIn, TOut>(func, blockOptions ?? new());
    _blocks.Add(new() { Value = newBlock, IsBlockAsync = false });
    return new(this);
  }

  public IntermediateBuilingBlock<TInitialIn, TOut> AddFirstAsyncBlock<TOut>(Func<TInitialIn, Task<TOut>> func, ExecutionDataflowBlockOptions? blockOptions = null)
  {
    if (_blocks.Count > 0)
    {
      throw new InvalidOperationException("Pipeline must be empty when adding the first block.");
    }

    var newBlock = new TransformBlock<TInitialIn, Task<TOut>>(func, blockOptions ?? new());
    _blocks.Add(new() { Value = newBlock, IsBlockAsync = true });
    return new(this);
  }

  internal IntermediateBuilingBlock<TInitialIn, TOut> AddBlock<TIn, TOut>(Func<TIn, TOut> func, PipelineBlockOptions? pipelineBlockOptions = null)
  {
    if (IsAsync(typeof(TOut)))
    {
      throw new InvalidOperationException($"Please use the method {nameof(IntermediateBuilingBlock<TInitialIn, TOut>.AddAsyncBlock)} for async operation.");
    }

    if (_blocks.Count == 0)
    {
      throw new InvalidOperationException("Expected pipeline to already have at least 1 block.");
    }

    var blockOptions = pipelineBlockOptions?.BlockOptions ?? new();
    var linkOptions = pipelineBlockOptions?.LinkOptions ?? new();
    var lastBlock = _blocks.Last();
    if (lastBlock.IsBlockAsync)
    {
      var newBlock = new TransformBlock<Task<TIn>, TOut>(async inputTask => func(await inputTask), blockOptions);
      var lastSrcBlock = _blocks.Last().Value as ISourceBlock<Task<TIn>>
        ?? throw new ArgumentException($"Cannot link block to the last async block in the pipeline due to output type mismatch. Invalid input type: {typeof(TIn).FullName}.");

      lastSrcBlock.LinkTo(newBlock, linkOptions);
      _blocks.Add(new() { Value = newBlock, IsBlockAsync = false });
    }
    else
    {
      var newBlock = new TransformBlock<TIn, TOut>(func, blockOptions);
      var lastSrcBlock = _blocks.Last().Value as ISourceBlock<TIn>
        ?? throw new ArgumentException($"Cannot link block to the last block in the pipeline due to output type mismatch. Invalid input type: {typeof(TIn).FullName}.");

      lastSrcBlock.LinkTo(newBlock, linkOptions);
      _blocks.Add(new() { Value = newBlock, IsBlockAsync = false });
    }
    return new(this);
  }

  internal IntermediateBuilingBlock<TInitialIn, TOut> AddAsyncBlock<TIn, TOut>(Func<TIn, Task<TOut>> func, PipelineBlockOptions? pipelineBlockOptions = null)
  {
    if (_blocks.Count == 0)
    {
      throw new InvalidOperationException("Expected pipeline to already have at least 1 block.");
    }

    IDataflowBlock newBlock;
    var lastBlock = _blocks.Last();

    var blockOptions = pipelineBlockOptions?.BlockOptions ?? new();
    var linkOptions = pipelineBlockOptions?.LinkOptions ?? new();
    if (lastBlock.IsBlockAsync)
    {
      var newTransformBlock = new TransformBlock<Task<TIn>, Task<TOut>>(async inputTask => func(await inputTask), blockOptions);
      var lastTargetBlock = _blocks.Last().Value as ISourceBlock<Task<TIn>>
        ?? throw new ArgumentException($"Cannot link block to the last async block in the pipeline due to output type mismatch. Invalid input type: {typeof(TIn).FullName}.");

      lastTargetBlock.LinkTo(newTransformBlock, linkOptions);
      newBlock = newTransformBlock;
    }
    else
    {
      var newTransformBlock = new TransformBlock<TIn, Task<TOut>>(func, blockOptions);
      var lastTargetBlock = _blocks.Last().Value as ISourceBlock<TIn>
        ?? throw new ArgumentException($"Cannot link block to the last block in the pipeline due to output type mismatch. Invalid input type: {typeof(TIn).FullName}.");

      lastTargetBlock.LinkTo(newTransformBlock, linkOptions);
      newBlock = newTransformBlock;
    }
 
    _blocks.Add(new() { Value = newBlock, IsBlockAsync = true });
    return new(this);
  }

  internal void AddLastBlock<TIn>(Action<TIn> action, PipelineBlockOptions? pipelineBlockOptions = null)
  {
    if (_blocks.Count == 0)
    {
      throw new InvalidOperationException("Expected pipeline to already have at least 1 block.");
    }

    var blockOptions = pipelineBlockOptions?.BlockOptions ?? new();
    var linkOptions = pipelineBlockOptions?.LinkOptions ?? new();
    var lastBlock = _blocks.Last();
    if (lastBlock.IsBlockAsync)
    {
      var newBlock = new ActionBlock<Task<TIn>>(async input => action(await input), blockOptions);
      var lastTargetBlock = _blocks.Last().Value as ISourceBlock<Task<TIn>>
        ?? throw new ArgumentException($"Cannot link block to the last async block in the pipeline due to output type mismatch. Invalid input type: {typeof(TIn).FullName}.");
      lastTargetBlock.LinkTo(newBlock, linkOptions);
      _blocks.Add(new() { Value = newBlock, IsBlockAsync = false });
    }
    else
    {
      var newBlock = new ActionBlock<TIn>(action, blockOptions);
      var lastTargetBlock = _blocks.Last().Value as ISourceBlock<TIn>
        ?? throw new ArgumentException($"Cannot link block to the last block in the pipeline due to output type mismatch. Invalid input type: {typeof(TIn).FullName}.");
      lastTargetBlock.LinkTo(newBlock, linkOptions);
      _blocks.Add(new() { Value = newBlock, IsBlockAsync = false });
    }       

    _lastBlockAdded = true;
  }

  internal void AddLastAsyncBlock<TIn>(Func<TIn, Task> func, PipelineBlockOptions? pipelineBlockOptions = null)
  {
    if (_blocks.Count == 0)
    {
      throw new InvalidOperationException("Expected pipeline to already have at least 1 block.");
    }

    var blockOptions = pipelineBlockOptions?.BlockOptions ?? new();
    var linkOptions = pipelineBlockOptions?.LinkOptions ?? new();
    var lastBlock = _blocks.Last();
    if (lastBlock.IsBlockAsync)
    {
      var newBlock = new ActionBlock<Task<TIn>>(async input => await func(await input), blockOptions);
      var lastTargetBlock = _blocks.Last().Value as ISourceBlock<Task<TIn>>
        ?? throw new ArgumentException($"Cannot link block to the last async block in the pipeline due to output type mismatch. Invalid input type: {typeof(TIn).FullName}.");
      lastTargetBlock.LinkTo(newBlock, linkOptions);
      _blocks.Add(new() { Value = newBlock, IsBlockAsync = true });
    }
    else
    {
      var newBlock = new ActionBlock<TIn>(func, blockOptions);
      var lastTargetBlock = _blocks.Last().Value as ISourceBlock<TIn>
        ?? throw new ArgumentException($"Cannot link block to the last block in the pipeline due to output type mismatch. Invalid input type: {typeof(TIn).FullName}.");
      lastTargetBlock.LinkTo(newBlock, linkOptions);
      _blocks.Add(new() { Value = newBlock, IsBlockAsync = true });
    }       

    _lastBlockAdded = true;
  }

  internal void Fork()
  {
    _lastBlockAdded = true;
  }

  internal void Branch<TIn>(Predicate<TIn> predicate, PipelineBuilder<TIn> branchPipelineBuilder, DataflowLinkOptions? linkOptions = null)
  {
    if (_blocks.Count == 0)
    {
      throw new InvalidOperationException("Expected pipeline to already have at least 1 block.");
    }

    // TODO: handle async
    var lastBlock = _blocks.Last().Value;
    var firstBlockSubPipeline = branchPipelineBuilder._blocks.First().Value as ITargetBlock<TIn>
      ?? throw new ArgumentException($"Cannot link branch pipeline to the last block in the pipeline due to output type mismatch. Invalid input type: {typeof(TIn).FullName}.");
    
    var lastSrcBlock = _blocks.Last().Value as ISourceBlock<TIn>
      ?? throw new ArgumentException($"Cannot link branch pipeline to the last block in the pipeline due to output type mismatch. Invalid input type: {typeof(TIn).FullName}.");

    lastSrcBlock.LinkTo(firstBlockSubPipeline, linkOptions ?? new(), predicate);
    ((IPipelineBuilder)this).BranchPipelineBuilders.Add(branchPipelineBuilder);
  }

  internal void Default<TIn>(PipelineBuilder<TIn> branchPipelineBuilder, DataflowLinkOptions? linkOptions = null)
  {
    if (_blocks.Count == 0)
    {
      throw new InvalidOperationException("Expected pipeline to already have at least 1 block.");
    }

    // TODO: handle async
    var lastBlock = _blocks.Last().Value;
    var firstBlockSubPipeline = branchPipelineBuilder._blocks.First().Value as ITargetBlock<TIn>
      ?? throw new ArgumentException($"Cannot link branch pipeline to the last block in the pipeline due to output type mismatch. Invalid input type: {typeof(TIn).FullName}.");
    
    var lastSrcBlock = _blocks.Last().Value as ISourceBlock<TIn>
      ?? throw new ArgumentException($"Cannot link branch pipeline to the last block in the pipeline due to output type mismatch. Invalid input type: {typeof(TIn).FullName}.");

    lastSrcBlock.LinkTo(firstBlockSubPipeline, linkOptions ?? new());
    ((IPipelineBuilder)this).BranchPipelineBuilders.Add(branchPipelineBuilder);
  }

  public Pipeline<TInitialIn> Build()
  {
    ValidateBeforeBuild();

    var firstBlock = _blocks.First().Value as ITargetBlock<TInitialIn>
      ?? throw new InvalidOperationException($"Input type of first block must match with type {typeof(TInitialIn).FullName}.");

    var leafBlocks = GetLeafBlocks();

    _pipelineBuilt = true;
    return new(firstBlock, leafBlocks.Select(block => block.Value));
  }

  private void ValidateBeforeBuild()
  {
    if (_pipelineBuilt)
    {
      throw new InvalidOperationException($"Pipeline already built. Please use a new {nameof(PipelineBuilder<TInitialIn>)} to build a new pipeline.");
    }

    if (_blocks.Count == 0)
    {
      throw new InvalidOperationException("Pipeline does not have any block defined.");
    }

    if (!_lastBlockAdded)
    {
      throw new InvalidOperationException($"Must call {nameof(IntermediateBuilingBlock<TInitialIn, object>.AddLastBlock)} " +
                                          "to indicate pipeline is ready to be built.");
    }
  }

  private IList<PipelineBlock> GetLeafBlocks()
  {
    var leafBlocks = new List<PipelineBlock>();
    FindLeaftBlocksRecursively(this, leafBlocks);
    return leafBlocks;

    static void FindLeaftBlocksRecursively(IPipelineBuilder currentPipelineBuilder, IList<PipelineBlock> leafBlocks)
    {
      if (!currentPipelineBuilder.BranchPipelineBuilders.Any())
      {
        leafBlocks.Add(currentPipelineBuilder.LastBlock);
        return;
      }

      foreach (var pipelineBuilder in currentPipelineBuilder.BranchPipelineBuilders)
      {
        FindLeaftBlocksRecursively(pipelineBuilder, leafBlocks);
      }
    }
  }

  private static bool IsAsync(Type type)
  {
    return type == typeof(Task) || 
            (type.IsGenericType && 
            type.GetGenericTypeDefinition() == typeof(Task<>));
  }
}
