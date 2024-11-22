using System.Threading.Tasks.Dataflow;

var linkOpts = new DataflowLinkOptions { PropagateCompletion = true };
var pipelineBuilder = new DataflowPipelineBuilder();
pipelineBuilder
  .AddFirstBlock<string, int>(int.Parse)
  .AddBlock(number => number*number, linkOptions: linkOpts)
  .AddBlock(number => number.ToString(), linkOptions: linkOpts)
  .AddLastBlock(Console.WriteLine, linkOptions: linkOpts)
  ;

var p = pipelineBuilder.Build<string>();
await p.ExecuteAsync(["1", "3", "5", "1032"]);
