namespace DataflowBuilder;

internal interface IPipelineBuilder
{
  PipelineBlock FirstBlock { get; }

  PipelineBlock LastBlock { get; }

  IList<IPipelineBuilder> BranchPipelineBuilders { get; }
}
