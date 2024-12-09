namespace DataflowBuilder;

internal interface IPipeline
{
  /// <summary>
  /// Id of a pipeline.
  /// </summary>
  string Id { get; }
  
  /// <summary>
  /// First block in the pipeline.
  /// </summary>
  PipelineBlock FirstBlock { get; }

  /// <summary>
  /// Last block in the pipeline.
  /// </summary>
  PipelineBlock LastBlock { get; }

  /// <summary>
  /// List of branch pipelines that this
  /// pipeline has forked into.
  /// </summary>
  IList<IPipeline> BranchPipelines { get; }

  /// <summary>
  /// Get called before a pipeline is built.
  /// </summary>
  void BeforeBuild();
}
