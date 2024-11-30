namespace DataflowBuilder;

public class PipelineBlockOptions
{
  public ExecutionDataflowBlockOptions? BlockOptions { get; init; }

  public DataflowLinkOptions? LinkOptions { get; init; }
}
