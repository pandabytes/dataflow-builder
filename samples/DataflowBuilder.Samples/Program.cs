
// var linkOpts = new DataflowLinkOptions { PropagateCompletion = true };
var pipelineBlockOpts = new PipelineBlockOptions { LinkOptions = new() { PropagateCompletion = true } };
var pipelineBuilder = new PipelineBuilder<string>();
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
  .AddFirstBlock(int.Parse)
  .AddBlock(number => number*number, pipelineBlockOpts)
  .AddBlock(number => number.ToString(), pipelineBlockOpts)
  .AddAsyncBlock(async str =>
  {
    await Task.Delay(1).ConfigureAwait(false);
    return $"[{str}]";
  }, pipelineBlockOpts)
  .AddAsyncBlock(async str =>
  {
    await Task.Delay(1).ConfigureAwait(false);
    return $"[{str}]";
  }, pipelineBlockOpts)
  .AddBlock(str => $"*{str}*", pipelineBlockOpts)
  .AddBlock(str =>
  {
    Thread.Sleep(1);
    return $"[{str}]";
  }, pipelineBlockOpts)
  // .AddLastBlock(Console.WriteLine, linkOptions: linkOpts)
  .AddLastAsyncBlock(async str =>
  {
    await Task.Delay(1000);
    Console.WriteLine(str);
  }, pipelineBlockOpts)
  ;

var p = pipelineBuilder.Build();
await p.ExecuteAsync(["1", "3", "5"]);
