namespace PipelineBuilder;

public class DataflowPipelineBuilder
{
  private class PipelineBlock
  {
    public required IDataflowBlock Value { get; init; }

    public required bool IsBlockAsync { get; init; }
  }

  private readonly IList<PipelineBlock> _blocks;

  private bool _pipelineBuilt;

  private bool _lastBlockAdded;

  public DataflowPipelineBuilder()
  {
    _blocks = new List<PipelineBlock>();
    _lastBlockAdded = false;
    _pipelineBuilt = false;
  }

  public IntermediateAddBlock<TOut> AddFirstBlock<TIn, TOut>(Func<TIn, TOut> func, ExecutionDataflowBlockOptions? blockOptions = null)
  {
    if (_blocks.Count > 0)
    {
      throw new InvalidOperationException("Pipeline must be empty when adding the first block.");
    }

    if (IsAsync(typeof(TOut)))
    {
      throw new InvalidOperationException($"Please use the method {nameof(IntermediateAddBlock<TIn>.AddAsyncBlock)} for async operation.");
    }

    var newBlock = new TransformBlock<TIn, TOut>(func, blockOptions ?? new());
    _blocks.Add(new() { Value = newBlock, IsBlockAsync = false });
    return new(this);
  }

  public IntermediateAddBlock<TOut> AddFirstAsyncBlock<TIn, TOut>(Func<TIn, Task<TOut>> func, ExecutionDataflowBlockOptions? blockOptions = null)
  {
    if (_blocks.Count > 0)
    {
      throw new InvalidOperationException("Pipeline must be empty when adding the first block.");
    }

    var newBlock = new TransformBlock<TIn, Task<TOut>>(func, blockOptions ?? new());
    _blocks.Add(new() { Value = newBlock, IsBlockAsync = true });
    return new(this);
  }

  internal IntermediateAddBlock<TOut> AddBlock<TIn, TOut>(Func<TIn, TOut> func, ExecutionDataflowBlockOptions? blockOptions = null, DataflowLinkOptions? linkOptions = null)
  {
    if (IsAsync(typeof(TOut)))
    {
      throw new InvalidOperationException($"Please use the method {nameof(IntermediateAddBlock<TIn>.AddAsyncBlock)} for async operation.");
    }

    if (_blocks.Count == 0)
    {
      throw new InvalidOperationException("Expected pipeline to already have at least 1 block.");
    }

    var lastBlock = _blocks.Last();
    if (lastBlock.IsBlockAsync)
    {
      var newBlock = new TransformBlock<Task<TIn>, TOut>(async inputTask => func(await inputTask), blockOptions ?? new());
      var lastSrcBlock = _blocks.Last().Value as ISourceBlock<Task<TIn>>
        ?? throw new ArgumentException($"Cannot link block to the last async block in the pipeline due to output type mismatch. Invalid input type: {typeof(TIn).FullName}.");

      lastSrcBlock.LinkTo(newBlock, linkOptions ?? new());
      _blocks.Add(new() { Value = newBlock, IsBlockAsync = false });
    }
    else
    {
      var newBlock = new TransformBlock<TIn, TOut>(func, blockOptions ?? new());
      var lastSrcBlock = _blocks.Last().Value as ISourceBlock<TIn>
        ?? throw new ArgumentException($"Cannot link block to the last block in the pipeline due to output type mismatch. Invalid input type: {typeof(TIn).FullName}.");

      lastSrcBlock.LinkTo(newBlock, linkOptions ?? new());
      _blocks.Add(new() { Value = newBlock, IsBlockAsync = false });
    }
    return new(this);
  }

  internal IntermediateAddBlock<TOut> AddAsyncBlock<TIn, TOut>(Func<TIn, Task<TOut>> func, ExecutionDataflowBlockOptions? blockOptions = null, DataflowLinkOptions? linkOptions = null)
  {
    if (_blocks.Count == 0)
    {
      throw new InvalidOperationException("Expected pipeline to already have at least 1 block.");
    }

    IDataflowBlock newBlock;
    var lastBlock = _blocks.Last();

    if (lastBlock.IsBlockAsync)
    {
      var newTransformBlock = new TransformBlock<Task<TIn>, Task<TOut>>(async input => func(await input), blockOptions ?? new());
      var lastTargetBlock = _blocks.Last().Value as ISourceBlock<Task<TIn>>
        ?? throw new ArgumentException($"Cannot link block to the last async block in the pipeline due to output type mismatch. Invalid input type: {typeof(TIn).FullName}.");

      lastTargetBlock.LinkTo(newTransformBlock, linkOptions ?? new());
      newBlock = newTransformBlock;
    }
    else
    {
      var newTransformBlock = new TransformBlock<TIn, Task<TOut>>(func, blockOptions ?? new());
      var lastTargetBlock = _blocks.Last().Value as ISourceBlock<TIn>
        ?? throw new ArgumentException($"Cannot link block to the last block in the pipeline due to output type mismatch. Invalid input type: {typeof(TIn).FullName}.");

      lastTargetBlock.LinkTo(newTransformBlock, linkOptions ?? new());
      newBlock = newTransformBlock;
    }

    _blocks.Add(new() { Value = newBlock, IsBlockAsync = true });
    return new(this);
  }

  internal void AddLastBlock<TIn>(Action<TIn> action, ExecutionDataflowBlockOptions? blockOptions = null, DataflowLinkOptions? linkOptions = null)
  {
    if (_blocks.Count == 0)
    {
      throw new InvalidOperationException("Expected pipeline to already have at least 1 block.");
    }

    var lastBlock = _blocks.Last();
    if (lastBlock.IsBlockAsync)
    {
      var newBlock = new ActionBlock<Task<TIn>>(async input => action(await input), blockOptions ?? new());
      var lastTargetBlock = _blocks.Last().Value as ISourceBlock<Task<TIn>>
        ?? throw new ArgumentException($"Cannot link block to the last async block in the pipeline due to output type mismatch. Invalid input type: {typeof(TIn).FullName}.");
      lastTargetBlock.LinkTo(newBlock, linkOptions ?? new());
      _blocks.Add(new() { Value = newBlock, IsBlockAsync = false });
    }
    else
    {
      var newBlock = new ActionBlock<TIn>(action, blockOptions ?? new());
      var lastTargetBlock = _blocks.Last().Value as ISourceBlock<TIn>
        ?? throw new ArgumentException($"Cannot link block to the last block in the pipeline due to output type mismatch. Invalid input type: {typeof(TIn).FullName}.");
      lastTargetBlock.LinkTo(newBlock, linkOptions ?? new());
      _blocks.Add(new() { Value = newBlock, IsBlockAsync = false });
    }       

    _lastBlockAdded = true;
  }

  internal void AddLastAsyncBlock<TIn>(Func<TIn, Task> func, ExecutionDataflowBlockOptions? blockOptions = null, DataflowLinkOptions? linkOptions = null)
  {
    if (_blocks.Count == 0)
    {
      throw new InvalidOperationException("Expected pipeline to already have at least 1 block.");
    }

    var lastBlock = _blocks.Last();
    if (lastBlock.IsBlockAsync)
    {
      var newBlock = new ActionBlock<Task<TIn>>(async input => await func(await input), blockOptions ?? new());
      var lastTargetBlock = _blocks.Last().Value as ISourceBlock<Task<TIn>>
        ?? throw new ArgumentException($"Cannot link block to the last async block in the pipeline due to output type mismatch. Invalid input type: {typeof(TIn).FullName}.");
      lastTargetBlock.LinkTo(newBlock, linkOptions ?? new());
      _blocks.Add(new() { Value = newBlock, IsBlockAsync = true });
    }
    else
    {
      var newBlock = new ActionBlock<TIn>(func, blockOptions ?? new());
      var lastTargetBlock = _blocks.Last().Value as ISourceBlock<TIn>
        ?? throw new ArgumentException($"Cannot link block to the last block in the pipeline due to output type mismatch. Invalid input type: {typeof(TIn).FullName}.");
      lastTargetBlock.LinkTo(newBlock, linkOptions ?? new());
      _blocks.Add(new() { Value = newBlock, IsBlockAsync = true });
    }       

    _lastBlockAdded = true;
  }

  public Pipeline<TIn> Build<TIn>()
  {
    ValidateBeforeBuild<TIn>();

    var firstBlock = _blocks.First().Value as ITargetBlock<TIn>
      ?? throw new InvalidOperationException($"Input type of first block must match with type {typeof(TIn).FullName}.");
    IEnumerable<IDataflowBlock> lastBlocks = [_blocks.Last().Value];

    _pipelineBuilt = true;
    return new(firstBlock, lastBlocks);
  }

  private void ValidateBeforeBuild<TIn>()
  {
    if (_pipelineBuilt)
    {
      throw new InvalidOperationException($"Pipeline already built. Please use a new {nameof(DataflowPipelineBuilder)} to build a new pipeline.");
    }

    if (_blocks.Count == 0)
    {
      throw new InvalidOperationException("Pipeline does not have any block defined.");
    }

    if (!_lastBlockAdded)
    {
      throw new InvalidOperationException($"Must call {nameof(IntermediateAddBlock<TIn>.AddLastBlock)} " +
                                          "to indicate pipeline is ready to be built.");
    }
  }

  private static bool IsAsync(Type type)
  {
    return type == typeof(Task) || 
            (type.IsGenericType && 
            type.GetGenericTypeDefinition() == typeof(Task<>));
  }

  #region Scratch

  internal IntermediateAddBlock<TOut> xxxAddAsyncBlock<TIn, TOut>(Func<TIn, Task<TOut>> func, ExecutionDataflowBlockOptions? blockOptions = null, DataflowLinkOptions? linkOptions = null)
  {
    // var newBlock = new TransformBlock<TIn, Task<TOut>>(func, blockOptions ?? new());
    // if (_blocks.Count > 0)
    // {
    //   var lastTargetBlock = _blocks.Last().Value as ISourceBlock<TIn>
    //     ?? throw new ArgumentException("Cannot link block to last block in the pipeline due to output type mismatch");
    //   lastTargetBlock.LinkTo(newBlock, linkOptions ?? new());        
    // }
    // _blocks.Add(new() { Value = newBlock, IsBlockAsync = true });

    var lastBlock = _blocks.Last();
    if (lastBlock.IsBlockAsync)
    {
      var newBlock = new TransformBlock<Task<TIn>, Task<TOut>>(async input => func(await input), blockOptions ?? new());
      var lastTargetBlock = _blocks.Last().Value as ISourceBlock<Task<TIn>>
        ?? throw new ArgumentException("Cannot link block to last async block in the pipeline due to output type mismatch. " + typeof(TIn).FullName);
      lastTargetBlock.LinkTo(newBlock, linkOptions ?? new());
      _blocks.Add(new() { Value = newBlock, IsBlockAsync = true });
    }
    else
    {
      System.Console.WriteLine("AddAsync - sync");
      var newBlock = new TransformBlock<TIn, TOut>(func, blockOptions ?? new());
      var lastTargetBlock = _blocks.Last().Value as ISourceBlock<TIn>
        ?? throw new ArgumentException("Cannot link block to last block in the pipeline due to output type mismatch. " + typeof(TIn).FullName);
      lastTargetBlock.LinkTo(newBlock, linkOptions ?? new());
      _blocks.Add(new() { Value = newBlock, IsBlockAsync = true });
    }

    return new(this);
  }

      // var newBlock = new TransformBlock<Task<TIn>, TOut>(async inputTask =>
      // {
      //   var result = await inputTask;
      //   return func(result);
      // }, blockOptions ?? new());

      // var lastTargetBlock = _blocks.Last() as ISourceBlock<Task<TIn>>
      //   ?? throw new ArgumentException("Cannot link block to last block in the pipeline due to output type mismatch.");
      // lastTargetBlock.LinkTo(newBlock, linkOptions ?? new());
      // _blocks.Add(newBlock);

      // return new IntermediateAddBlock<int>(this);

  #endregion
}
