namespace DataflowBuilder;

public sealed class Pipeline<TInitialIn> : IPipeline
{
  private readonly IList<PipelineBlock> _blocks;

  private PipelineBuildStatus _buildStatus;

  private readonly IList<IPipeline> _branchPipelines;

  /// <inheritdoc/>
  PipelineBlock IPipeline.FirstBlock => _blocks.First();

  /// <inheritdoc/>
  PipelineBlock IPipeline.LastBlock => _blocks.Last();

  /// <inheritdoc/>
  IList<IPipeline> IPipeline.BranchPipelines => _branchPipelines;

  /// <inheritdoc/>
  void IPipeline.BeforeBuild()
  {
    if (_blocks.Count == 0)
    {
      throw new InvalidOperationException("Pipeline does not have any block defined.");
    }

    switch (_buildStatus)
    {
      case PipelineBuildStatus.Built:
        throw new InvalidOperationException($"Pipeline already built. Please create a new " +
                                            $"{nameof(Pipeline<TInitialIn>)} to build a new pipeline.");
      case PipelineBuildStatus.Progress:
        throw new InvalidOperationException($"Must call {nameof(IntermediateBuildingBlock<TInitialIn, object>.AddLastBlock)} " +
                                            "to indicate pipeline is ready to be built.");
      case PipelineBuildStatus.Forked:
        if (AsIPipeline().BranchPipelines.Count == 0)
        {
          throw new InvalidOperationException("Pipeline was forked but it was not provided any branch.");
        }
        break;
    }

    foreach (var branchPipeline in AsIPipeline().BranchPipelines)
    {
      try
      {
        branchPipeline.BeforeBuild();
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException($"Branch pipeline \"{branchPipeline.Id}\" failed to be built.", ex);
      }
    }
  }

  /// <inheritdoc/>
  public string Id { get; }

  public Pipeline(string id)
  {
    if (string.IsNullOrWhiteSpace(id))
    {
      throw new ArgumentException("Pipeline id must not be empty or null.", nameof(id));
    }

    Id = id;
    _blocks = new List<PipelineBlock>();
    _branchPipelines = new List<IPipeline>();
    _buildStatus = PipelineBuildStatus.Progress;
  }

  public IntermediateBuildingBlock<TInitialIn, TOut> AddFirstBlock<TOut>(
    Func<TInitialIn, TOut> func,
    ExecutionDataflowBlockOptions? blockOptions = null
  )
  {
    if (_blocks.Count > 0)
    {
      throw new InvalidOperationException("Pipeline must be empty when adding the first block.");
    }

    if (IsAsync(typeof(TOut)))
    {
      throw new InvalidOperationException($"Please use the method {nameof(IntermediateBuildingBlock<TInitialIn, TOut>.AddAsyncBlock)} for async operation.");
    }

    var newBlock = new TransformBlock<TInitialIn, TOut>(func, blockOptions ?? new());
    _blocks.Add(new() { Value = newBlock, IsBlockAsync = false });
    return new(this);
  }

  public IntermediateBuildingBlock<TInitialIn, TOut> AddFirstAsyncBlock<TOut>(
    Func<TInitialIn, Task<TOut>> func,
    ExecutionDataflowBlockOptions? blockOptions = null
  )
  {
    if (_blocks.Count > 0)
    {
      throw new InvalidOperationException("Pipeline must be empty when adding the first block.");
    }

    var newBlock = new TransformBlock<TInitialIn, Task<TOut>>(func, blockOptions ?? new());
    _blocks.Add(new() { Value = newBlock, IsBlockAsync = true });
    return new(this);
  }

  internal void AddBlock<TIn, TOut>(
    Func<TIn, TOut> func,
    PipelineBlockOptions? pipelineBlockOptions = null,
    bool allowTaskOutput = false
  )
  {
    if (!allowTaskOutput && IsAsync(typeof(TOut)))
    {
      throw new InvalidOperationException($"Please use the method {nameof(IntermediateBuildingBlock<TInitialIn, TOut>.AddAsyncBlock)} for async operation.");
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
  }

  internal void AddAsyncBlock<TIn, TOut>(
    Func<TIn, Task<TOut>> func,
    PipelineBlockOptions? pipelineBlockOptions = null
  )
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
  }

  internal void AddLastBlock<TIn>(
    Action<TIn> action,
    PipelineBlockOptions? pipelineBlockOptions = null
  )
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

    _buildStatus = PipelineBuildStatus.ReadyForBuild;
  }

  internal void AddLastAsyncBlock<TIn>(
    Func<TIn, Task> func,
    PipelineBlockOptions? pipelineBlockOptions = null
  )
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

    _buildStatus = PipelineBuildStatus.ReadyForBuild;
  }

  internal void Fork() => _buildStatus = PipelineBuildStatus.Forked;

  internal void BranchOrDefault<TIn>(
    Pipeline<TIn> branchPipeline,
    Predicate<TIn>? predicate = null,
    DataflowLinkOptions? linkOptions = null
  )
  {
    if (_blocks.Count == 0)
    {
      throw new InvalidOperationException("Expected pipeline to already have at least 1 block.");
    }

    var lastBlock = _blocks.Last();
    if (lastBlock.IsBlockAsync)
    {
      var taskType = lastBlock.Value
        .GetType()
        .GetGenericArguments()
        .First(IsAsync);
      var taskResultType = GetTaskResultType(taskType);

      var branchPipelineType = branchPipeline.GetType().GetGenericArguments().First();

      throw new InvalidOperationException($@"
        Last block in pipeline contains an async operation, in which cannot
        be connected to the first block of branch pipeline ""{branchPipeline.Id}"".
        Because Task<{taskResultType.Name}> is not the input type {branchPipelineType.Name}
        that branch pipeline ""{branchPipeline.Id}"" expects.
      ");
    }

    var firstBlockBranchPipeline = branchPipeline.AsIPipeline().FirstBlock.Value as ITargetBlock<TIn>
      ?? throw new ArgumentException($"Cannot link branch pipeline to the last block in the pipeline due to output type mismatch. Invalid input type: {typeof(TIn).FullName}.");
    
    var lastSrcBlock = AsIPipeline().LastBlock.Value as ISourceBlock<TIn>
      ?? throw new ArgumentException($"Cannot link branch pipeline to the last block in the pipeline due to output type mismatch. Invalid input type: {typeof(TIn).FullName}.");

    if (predicate is null)
    {
      lastSrcBlock.LinkTo(firstBlockBranchPipeline, linkOptions ?? new());
    }
    else
    {
      lastSrcBlock.LinkTo(firstBlockBranchPipeline, linkOptions ?? new(), predicate);
    }

    ((IPipeline)this).BranchPipelines.Add(branchPipeline);
  }

  internal void Broadcast<TIn>(DataflowLinkOptions? linkOptions = null)
  {
    var lastSrcBlock = AsIPipeline().LastBlock.Value as ISourceBlock<TIn>
      ?? throw new ArgumentException($"Cannot link broadcast block to the last block in the pipeline due to output type mismatch. Invalid input type: {typeof(TIn).FullName}.");

    var broadcastBlock = new BroadcastBlock<TIn>(null);
    lastSrcBlock.LinkTo(broadcastBlock, linkOptions ?? new());
    _blocks.Add(new() { Value = broadcastBlock, IsBlockAsync = false });

    _buildStatus = PipelineBuildStatus.Forked;
  }

  public PipelineRunner<TInitialIn> Build()
  {
    AsIPipeline().BeforeBuild();

    var firstBlock = _blocks.First().Value as ITargetBlock<TInitialIn>
      ?? throw new InvalidOperationException($"Input type of first block must match with type {typeof(TInitialIn).FullName}.");

    var lastBlocks = GetLastBlocks().Select(block => block.Value);

    _buildStatus = PipelineBuildStatus.Built;
    return new(firstBlock, lastBlocks);
  }

  private IList<PipelineBlock> GetLastBlocks()
  {
    var leafBlocks = new List<PipelineBlock>();
    FindLastBlocksRecursively(this, leafBlocks);
    return leafBlocks;

    static void FindLastBlocksRecursively(IPipeline currentPipeline, IList<PipelineBlock> leafBlocks)
    {
      if (!currentPipeline.BranchPipelines.Any())
      {
        leafBlocks.Add(currentPipeline.LastBlock);
        return;
      }

      foreach (var pipeline in currentPipeline.BranchPipelines)
      {
        FindLastBlocksRecursively(pipeline, leafBlocks);
      }
    }
  }

  private IPipeline AsIPipeline() => this;

  private static bool IsAsync(Type type)
  {
    return type == typeof(Task) || 
            (type.IsGenericType && 
            type.GetGenericTypeDefinition() == typeof(Task<>));
  }

  private static Type GetTaskResultType(Type taskType)
  {
    if (!IsAsync(taskType))
    {
      throw new Exception("oh no");
    }

    var genericTypes = taskType.GetGenericArguments();
    return genericTypes.First();
  }

  private static void ShowGenericTypes(Type type)
  {
    foreach (var genericType in type.GetGenericArguments())
    {
      System.Console.WriteLine(genericType.FullName);
    }
  }
}
