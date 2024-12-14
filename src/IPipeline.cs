namespace DataflowBuilder;

/// <summary>
/// Pipeline interface.
/// </summary>
public interface IPipeline
{
  /// <summary>
  /// Id of the pipeline.
  /// </summary>
  string Id { get; }

  /// <summary>
  /// Blocks in the pipeline.
  /// </summary>
  IReadOnlyList<PipelineBlock> Blocks { get; }

  /// <summary>
  /// List of branch pipelines that this
  /// pipeline has forked into.
  /// </summary>
  IReadOnlyList<IPipeline> BranchPipelines { get; }

  /// <summary>
  /// Get called before a pipeline is built.
  /// </summary>
  internal void BeforeBuild();
}
