namespace DataflowBuilder;

internal class PipelineBlock
{
  public required IDataflowBlock Value { get; init; }

  public required bool IsBlockAsync { get; init; }
}
