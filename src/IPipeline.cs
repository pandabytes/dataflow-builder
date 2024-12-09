namespace DataflowBuilder;

internal interface IPipeline
{
  string Id { get; }
  
  PipelineBlock FirstBlock { get; }

  PipelineBlock LastBlock { get; }

  IList<IPipeline> BranchPipelines { get; }

  void SetupForBuild();
}
