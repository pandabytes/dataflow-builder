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
    graph.Add(new DotNode()
      .WithIdentifier("start")
      .WithLabel("Start")
    );

    graph.Add(new DotEdge()
      .From("start")
      .To(firstNodeId)
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

    var firstNodeId = GetDotNodeId(pipeline.Id, "0");
    var lastNodeId = GetDotNodeId(pipeline.Id, (pipeline.Blocks.Count - 1).ToString());
    return (firstNodeId, lastNodeId, subgraph);
  }

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
}
