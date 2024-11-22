namespace PipelineBuilder;

// public class IntermediateAddBlock<TIn>
// {
//   protected BasePipelineBuilder Builder { get; }

//   internal IntermediateAddBlock(BasePipelineBuilder pipelineBuilder)
//   {
//     Builder = pipelineBuilder;
//   }

//   public IntermediateAddBlock<TOut> AddBlock<TOut>(Func<TIn, TOut> func, ExecutionDataflowBlockOptions? blockOptions = null, DataflowLinkOptions? linkOptions = null)
//   {
//     return new(Builder);
//   }
// }

public class IntermediateAddBlock<TIn>
{
  protected DataflowPipelineBuilder Builder { get; }

  internal IntermediateAddBlock(DataflowPipelineBuilder pipelineBuilder)
  {
    Builder = pipelineBuilder;
  }

  public IntermediateAddBlock<TOut> AddBlock<TOut>(Func<TIn, TOut> func, ExecutionDataflowBlockOptions? blockOptions = null, DataflowLinkOptions? linkOptions = null)
  {
    Builder.AddBlock(func, blockOptions, linkOptions);
    return new(Builder);
  }

  public void AddLastBlock(Action<TIn> action, ExecutionDataflowBlockOptions? blockOptions = null, DataflowLinkOptions? linkOptions = null)
  {
    Builder.AddLastBlock(action, blockOptions, linkOptions);
  }
}
