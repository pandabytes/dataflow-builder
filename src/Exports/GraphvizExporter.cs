using DotNetGraph.Compilation;
using DotNetGraph.Core;
using DotNetGraph.Extensions;

namespace DataflowBuilder.Exports;

internal class GraphvizExporter
{
  /// <summary>
  /// 
  /// </summary>
  /// <returns></returns>
  public async Task<string> ExportAsync(IPipeline pipeline)
  {
    var graph = new DotGraph()
      .WithIdentifier("Pipeline")
      .WithRankDir(DotRankDir.LR)
      .Directed();

    var (firstNodeId, _) = AddSubgraphRecursively(pipeline, graph, 0);

    // Add a start node which represent
    // the starting point where input
    // is fed into the pipeline
    const string startNodeId = "start_node_id";
    graph.Add(new DotNode()
      .WithIdentifier(startNodeId)
      .WithLabel("Start")
    );

    var (inputTypeName, _) = GetBlockTypeNames(pipeline.Blocks[0].Value);
    graph.Add(new DotEdge()
      .From(startNodeId)
      .To(firstNodeId)
      .WithLabel(inputTypeName)
    );

    await using var writer = new StringWriter();
    var context = new CompilationContext(writer, new CompilationOptions());
    await graph.CompileAsync(context);
    return writer.GetStringBuilder().ToString();
  }

  private static (string, string, DotSubgraph) ToDotSubgraph(IPipeline pipeline, int pipelineNumber)
  {
    var subgraph = new DotSubgraph()
      .WithLabel(pipeline.Id)
      .WithIdentifier($"cluster_{pipelineNumber}_{pipeline.Id}");

    // Add nodes and edges in the pipeline
    for (int i = 0; i < pipeline.Blocks.Count - 1; i++)
    {
      var fromNodeId = GetDotNodeId(pipeline.Id, i.ToString());
      var toNodeId = GetDotNodeId(pipeline.Id, (i + 1).ToString());

      var fromDotNode = new DotNode()
        .WithIdentifier(fromNodeId)
        .WithLabel(i.ToString())
        .WithShape("box");

      var toDotNode = new DotNode()
        .WithIdentifier(toNodeId)
        .WithLabel((i + 1).ToString())
        .WithShape("box");

      subgraph.Add(fromDotNode);
      subgraph.Add(toDotNode);

      // Add edge
      var (_, outputTypeName) = GetBlockTypeNames(pipeline.Blocks[i].Value);
      var dotEdge = new DotEdge()
        .From(fromNodeId)
        .To(toNodeId)
        // Label is the output type of the previous block
        .WithLabel(outputTypeName);

      subgraph.Add(dotEdge);
    }

    var firstNodeId = GetDotNodeId(pipeline.Id, "0");
    var lastNodeId = GetDotNodeId(pipeline.Id, (pipeline.Blocks.Count - 1).ToString());
    return (firstNodeId, lastNodeId, subgraph);
  }

  /// <summary>
  /// Add subgraph to the <paramref name="graph"/>
  /// recursively. Effectivly building the graphviz graph.
  /// </summary>
  /// <param name="pipeline"></param>
  /// <param name="graph"></param>
  /// <param name="pipelineNumber">This is used to make each pipeline unique.</param>
  /// <returns>
  /// Tuple where 1st item is the graphviz node id of
  /// the first block in the pipeline, and 2nd item
  /// is the graphviz node id of the last block.
  /// </returns>
  private static (string, string) AddSubgraphRecursively(IPipeline pipeline, DotGraph graph, int pipelineNumber)
  {
    var (firstNodeId, lastNodeId, subgraph) = ToDotSubgraph(pipeline, pipelineNumber);
    graph.Add(subgraph);

    foreach (var branchPipeline in pipeline.BranchPipelines)
    {
      var (firstBranchNodeId, _) = AddSubgraphRecursively(branchPipeline, graph, pipelineNumber++);
      graph.Add(new DotEdge()
        .From(lastNodeId)
        .To(firstBranchNodeId)
      );
    }

    return (firstNodeId, lastNodeId);
  }

  private static string GetDotNodeId(string pipelineId, string blockId)
    => $"node_{pipelineId}_{blockId}";

  /// <summary>
  /// Return a tuple where 1st item is the
  /// input type name of the block and 2nd
  /// item is the output type name of
  /// the block.
  /// </summary>
  /// <exception cref="NotSupportedException"></exception>
  private static (string, string) GetBlockTypeNames(IDataflowBlock block)
  {
    var blockType = block.GetType();
    var blockGenericTypeDef = blockType.GetGenericTypeDefinition();

    if (blockGenericTypeDef == typeof(TransformBlock<,>)
        || blockGenericTypeDef == typeof(TransformManyBlock<,>))
    {
      
      var firstGenericType = blockType.GetGenericArguments().First();
      var lastGenericType = blockType.GetGenericArguments().Last();

      string inputTypeName = firstGenericType.Name;
      if (firstGenericType.IsAsync())
      {
        inputTypeName = $"Task<{firstGenericType.GetTaskResultType().Name}>";
      }

      string outputTypeName = lastGenericType.Name;
      if (lastGenericType.IsAsync())
      {
        outputTypeName = $"Task<{lastGenericType.GetTaskResultType().Name}>";
      }

      return (inputTypeName, outputTypeName);
    }

    throw new NotSupportedException($"{blockType} not supported.");
  }
}
