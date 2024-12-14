using DotNetGraph.Compilation;
using DotNetGraph.Core;
using DotNetGraph.Extensions;

namespace DataflowBuilder;

/// <summary>
/// Pipeline consisting of TPL Dataflow blocks.
/// </summary>
/// <typeparam name="TPipelineFirstIn"></typeparam>
public sealed class Pipeline<TPipelineFirstIn> : IPipeline
{
  private readonly List<PipelineBlock> _blocks;

  private readonly List<IPipeline> _branchPipelines;

  private PipelineBuildStatus _buildStatus;

  /// <inheritdoc/>
  public IReadOnlyList<PipelineBlock> Blocks => _blocks;

  /// <inheritdoc/>
  public IReadOnlyList<IPipeline> BranchPipelines => _branchPipelines;

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
                                            $"{nameof(Pipeline<TPipelineFirstIn>)} to build a new pipeline.");
      case PipelineBuildStatus.Progress:
        throw new InvalidOperationException($"Must call {nameof(IntermediateBuildingBlock<TPipelineFirstIn, object>.AddLastBlock)} " +
                                            "to indicate pipeline is ready to be built.");
      case PipelineBuildStatus.Forked:
        if (_branchPipelines.Count == 0)
        {
          throw new InvalidOperationException("Pipeline was forked but it was not provided any branch.");
        }
        break;
    }

    foreach (var branchPipeline in _branchPipelines)
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

  /// <summary>
  /// Constructor.
  /// </summary>
  /// <param name="id">Pipeline id.</param>
  /// <exception cref="ArgumentException">Thrown when id is null or empty.</exception>
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

  /// <summary>
  /// Add the first synchronous block to the pipeline. If <typeparamref name="TOut"/> is
  /// <see cref="Task"/>, an exception will be thrown. This means you should
  /// use method <see cref="AddFirstAsyncBlock{TOut}(Func{TPipelineFirstIn, Task{TOut}}, ExecutionDataflowBlockOptions?)"/>
  /// instead.
  /// </summary>
  /// <typeparam name="TOut">Output type that this block produces.</typeparam>
  /// <param name="func">The logic of this block.</param>
  /// <param name="pipelineBlockOptions">Pipeline block options.</param>
  /// <returns></returns>
  /// <exception cref="InvalidOperationException"></exception>
  public IntermediateBuildingBlock<TPipelineFirstIn, TOut> AddFirstBlock<TOut>(
    Func<TPipelineFirstIn, TOut> func,
    ExecutionDataflowBlockOptions? pipelineBlockOptions = null
  )
  {
    if (_blocks.Count > 0)
    {
      throw new InvalidOperationException("Pipeline must be empty when adding the first block.");
    }

    if (IsAsync(typeof(TOut)))
    {
      throw new InvalidOperationException($"Please use the method {nameof(IntermediateBuildingBlock<TPipelineFirstIn, TOut>.AddAsyncBlock)} for async operation.");
    }

    var newBlock = new TransformBlock<TPipelineFirstIn, TOut>(func, pipelineBlockOptions ?? new());
    _blocks.Add(new() { Value = newBlock, IsBlockAsync = false });
    return new(this);
  }

  /// <summary>
  /// Add the first asynchronous block to the pipeline. Normally when a block produces a <see cref="Task"/>
  /// object, the next block's input type must be <see cref="Task"/> as well. But
  /// this method removes "<see cref="Task"/>" and instead it "returns" the type
  /// inside <see cref="Task"/>. 
  /// </summary>
  /// <typeparam name="TOut">Output type that this block produces.</typeparam>
  /// <param name="func">The logic of this block.</param>
  /// <param name="pipelineBlockOptions">Pipeline block options.</param>
  /// <returns></returns>
  /// <exception cref="InvalidOperationException"></exception>
  public IntermediateBuildingBlock<TPipelineFirstIn, TOut> AddFirstAsyncBlock<TOut>(
    Func<TPipelineFirstIn, Task<TOut>> func,
    ExecutionDataflowBlockOptions? pipelineBlockOptions = null
  )
  {
    if (_blocks.Count > 0)
    {
      throw new InvalidOperationException("Pipeline must be empty when adding the first block.");
    }

    var newBlock = new TransformBlock<TPipelineFirstIn, Task<TOut>>(func, pipelineBlockOptions ?? new());
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
      throw new InvalidOperationException($"Please use the method {nameof(IntermediateBuildingBlock<TPipelineFirstIn, TOut>.AddAsyncBlock)} for async operation.");
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

  internal void AddManyBlock<TIn, TOut>(
    Func<TIn, IEnumerable<TOut>> func,
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
      var newBlock = new TransformManyBlock<Task<TIn>, TOut>(async inputTask => func(await inputTask), blockOptions);
      var lastSrcBlock = _blocks.Last().Value as ISourceBlock<Task<TIn>>
        ?? throw new ArgumentException($"Cannot link many block to the last async block in the pipeline due to output type mismatch. Invalid input type: {typeof(TIn).FullName}.");

      lastSrcBlock.LinkTo(newBlock, linkOptions);
      _blocks.Add(new() { Value = newBlock, IsBlockAsync = false });
    }
    else
    {
      var newBlock = new TransformManyBlock<TIn, TOut>(func, blockOptions);
      var lastSrcBlock = _blocks.Last().Value as ISourceBlock<TIn>
        ?? throw new ArgumentException($"Cannot link many block to the last block in the pipeline due to output type mismatch. Invalid input type: {typeof(TIn).FullName}.");

      lastSrcBlock.LinkTo(newBlock, linkOptions);
      _blocks.Add(new() { Value = newBlock, IsBlockAsync = false });
    }
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

    var firstBlockBranchPipeline = branchPipeline._blocks[0].Value as ITargetBlock<TIn>
      ?? throw new ArgumentException($"Cannot link branch pipeline to the last block in the pipeline due to output type mismatch. Invalid input type: {typeof(TIn).FullName}.");
    
    var lastSrcBlock = _blocks[^1].Value as ISourceBlock<TIn>
      ?? throw new ArgumentException($"Cannot link branch pipeline to the last block in the pipeline due to output type mismatch. Invalid input type: {typeof(TIn).FullName}.");

    if (predicate is null)
    {
      lastSrcBlock.LinkTo(firstBlockBranchPipeline, linkOptions ?? new());
    }
    else
    {
      lastSrcBlock.LinkTo(firstBlockBranchPipeline, linkOptions ?? new(), predicate);
    }

    _branchPipelines.Add(branchPipeline);
  }

  internal void Broadcast<TIn>(
    Func<TIn, TIn>? cloningFunc = null,
    PipelineBlockOptions? pipelineBlockOptions = null
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

      throw new InvalidOperationException($@"
        Last block in pipeline contains an async operation, in which cannot
        be connected to the broadcast block. Because Task<{taskResultType.Name}>
        is not the input type {typeof(TIn).Name} that the broadcast block expects.
      ");
    }

    var broadcastBlock = new BroadcastBlock<TIn>(cloningFunc, pipelineBlockOptions?.BlockOptions ?? new());
    var lastSrcBlock = _blocks[^1].Value as ISourceBlock<TIn>
      ?? throw new ArgumentException($"Cannot link broadcast block to the last block in the pipeline due to output type mismatch. Invalid input type: {typeof(TIn).FullName}.");

    lastSrcBlock.LinkTo(broadcastBlock, pipelineBlockOptions?.LinkOptions ?? new());
    _blocks.Add(new() { Value = broadcastBlock, IsBlockAsync = false });

    _buildStatus = PipelineBuildStatus.Forked;
  }

  /// <summary>
  /// Build the pipeline. A pipeline can only be built once,
  /// if calling this method again, it throws an exception.
  /// </summary>
  /// <returns>A pipeline runner object.</returns>
  /// <exception cref="InvalidOperationException"></exception>
  public PipelineRunner<TPipelineFirstIn> Build()
  {
    ((IPipeline)this).BeforeBuild();

    var firstBlock = _blocks.First().Value as ITargetBlock<TPipelineFirstIn>
      ?? throw new InvalidOperationException($"Input type of first block must match with type {typeof(TPipelineFirstIn).FullName}.");

    var lastBlocks = GetLastBlocks().Select(block => block.Value);

    _buildStatus = PipelineBuildStatus.Built;
    return new(firstBlock, lastBlocks);
  }

  /// <summary>
  /// 
  /// </summary>
  /// <returns></returns>
  public async Task<string> ToGraphVizAsync()
  {
    var graph = new DotGraph()
      .WithIdentifier("Pipeline")
      .WithRankDir(DotRankDir.LR)
      .Directed();

    var subgraph = new DotSubgraph()
      .WithIdentifier("x");
      
    graph.Add(subgraph);

    subgraph.Add(new DotNode()
      .WithIdentifier("first")
      .WithLabel("Start")
    );

    for (int i = 0; i < _blocks.Count - 1; i++)
    {
      var fromNodeId = i.ToString();
      var toNodeId = (i + 1).ToString();

      var fromDotNode = new DotNode()
        .WithIdentifier(fromNodeId)
        .WithLabel(fromNodeId)
        .WithShape("box");

      var toDotNode = new DotNode()
        .WithIdentifier(toNodeId)
        .WithLabel(toNodeId)
        .WithShape("box");

      subgraph.Add(fromDotNode);
      subgraph.Add(toDotNode);

      // Add edge
      var dotEdge = new DotEdge()
        .From(fromNodeId)
        .To(toNodeId)
        .WithLabel("x");

      subgraph.Add(dotEdge);
    }

    subgraph.Add(new DotEdge()
      .From("first")
      .To("0")
      .WithLabel("x")
    );

    await using var writer = new StringWriter();
    var context = new CompilationContext(writer, new CompilationOptions());
    await graph.CompileAsync(context);
    return writer.GetStringBuilder().ToString();
  }

  private static DotSubgraph ToDotSubgraph(IPipeline pipeline)
  {
    var subgraph = new DotSubgraph().WithIdentifier(pipeline.Id);

    return subgraph;
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
        leafBlocks.Add(currentPipeline.Blocks[^1]);
        return;
      }

      foreach (var pipeline in currentPipeline.BranchPipelines)
      {
        FindLastBlocksRecursively(pipeline, leafBlocks);
      }
    }
  }

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
