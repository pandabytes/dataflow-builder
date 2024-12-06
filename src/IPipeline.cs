namespace DataflowBuilder;

internal interface IPipeline
{
  PipelineBlock FirstBlock { get; }

  PipelineBlock LastBlock { get; }

  IList<IPipeline> BranchPipelines { get; }
}
