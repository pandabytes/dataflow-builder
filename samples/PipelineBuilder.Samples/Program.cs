using System.Threading.Tasks.Dataflow;

var linkOpts = new DataflowLinkOptions { PropagateCompletion = true };
var pipelineBuilder = new DataflowPipelineBuilder();
pipelineBuilder
  // ******* Begin with async
  // .AddFirstAsyncBlock<string, int>(async str =>
  // {
  //   await Task.Delay(250);
  //   return int.Parse(str);
  // })
  // .AddAsyncBlock(async number =>
  // {
  //   await Task.Delay(250);
  //   return number;
  // }, linkOptions: linkOpts)
  // .AddBlock(number => number*2, linkOptions: linkOpts)
  // ******* Begin with sync
  .AddFirstBlock<string, int>(int.Parse)
  .AddBlock(number => number*number, linkOptions: linkOpts)
  .AddBlock(number => number.ToString(), linkOptions: linkOpts)
  .AddAsyncBlock(async str =>
  {
    await Task.Delay(1).ConfigureAwait(false);
    return $"[{str}]";
  }, linkOptions: linkOpts)
  .AddAsyncBlock(async str =>
  {
    await Task.Delay(1).ConfigureAwait(false);
    return $"[{str}]";
  }, linkOptions: linkOpts)
  .AddBlock(str => $"*{str}*", linkOptions: linkOpts)
  .AddBlock(str =>
  {
    Thread.Sleep(1);
    return $"[{str}]";
  }, linkOptions: linkOpts)
  // .AddLastBlock(Console.WriteLine, linkOptions: linkOpts)
  .AddLastAsyncBlock(async str =>
  {
    await Task.Delay(1000);
    Console.WriteLine(str);
  }, linkOptions: linkOpts)
  ;

var p = pipelineBuilder.Build<string>();
await p.ExecuteAsync(["1", "3", "5"]);
