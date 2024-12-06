
// var linkOpts = new DataflowLinkOptions { PropagateCompletion = true };
var pipelineBlockOpts = new PipelineBlockOptions { LinkOptions = new() { PropagateCompletion = true } };
var pipelineBuilder = new Pipeline<string>();

var sub1 = new Pipeline<int>();
  sub1
    .AddFirstBlock(number => number)
    .AddLastBlock(Console.WriteLine, pipelineBlockOpts);

var sub2 = new Pipeline<int>();
  sub2
    .AddFirstBlock(number => number)
    .AddLastBlock(Console.WriteLine, pipelineBlockOpts);

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
  // .AddBlock(number => number*number, pipelineBlockOpts)
  // .AddBlock(number => number.ToString(), pipelineBlockOpts)
  // .AddAsyncBlock(async str =>
  // {
  //   await Task.Delay(1).ConfigureAwait(false);
  //   return $"[{str}]";
  // }, pipelineBlockOpts)
  // .AddAsyncBlock(async str =>
  // {
  //   await Task.Delay(1).ConfigureAwait(false);
  //   return $"[{str}]";
  // }, pipelineBlockOpts)
  // .AddBlock(str => $"*{str}*", pipelineBlockOpts)
  // .AddBlock(str =>
  // {
  //   Thread.Sleep(1);
  //   return $"[{str}]";
  // }, pipelineBlockOpts)
  // .AddLastBlock(Console.WriteLine, linkOptions: linkOpts)
  .Fork()
    .Branch(n => n % 2 == 0, sub1)
    .Branch(n => n % 2 != 0, sub2)
  // .AddLastAsyncBlock(async str =>
  // {
  //   await Task.Delay(1000);
  //   Console.WriteLine(str);
  // }, pipelineBlockOpts)
  ;

// var p = pipelineBuilder.Build();
// await p.ExecuteAsync(["1", "3", "5"]);

await FooAsync();

static async Task FooAsync()
{
  var pipelineBlockOpts = new PipelineBlockOptions
  {
    BlockOptions = new() { MaxDegreeOfParallelism = 1 },
    LinkOptions = new() { PropagateCompletion = true }
  };

  var branch1 = new Pipeline<int>();
    branch1
      .AddFirstBlock(number => number)
      .AddLastBlock(n => Console.WriteLine($"Divisible by 2 {n}"), pipelineBlockOpts);

  var branch2 = new Pipeline<int>();
    branch2
      .AddFirstBlock(number => number)
      .AddLastBlock(n => Console.WriteLine($"Divisible by 5 {n}"), pipelineBlockOpts);

  var branch3 = new Pipeline<int>();
    branch3
      .AddFirstBlock(number => number)
      .AddLastBlock(n => Console.WriteLine($"ALL {n}"), pipelineBlockOpts);

  var pipeline = new Pipeline<string>();
  pipeline
    .AddFirstBlock(int.Parse)
    .AddBlock(number => number*number, pipelineBlockOpts)
    .Fork()
      .Branch(n => n % 2 == 0, branch1, pipelineBlockOpts.LinkOptions)
      .Branch(n => n % 5 == 0, branch2, pipelineBlockOpts.LinkOptions)
      .Default(branch3, pipelineBlockOpts.LinkOptions);
    ;

  var pipelineRunner = pipeline.Build();
  await pipelineRunner.ExecuteAsync(["1", "2", "3", "4", "5", "6"]);
}
