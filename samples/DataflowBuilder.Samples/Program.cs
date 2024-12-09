
// var linkOpts = new DataflowLinkOptions { PropagateCompletion = true };
var pipelineBlockOpts = new PipelineBlockOptions { LinkOptions = new() { PropagateCompletion = true } };
var pipelineBuilder = new Pipeline<string>("A");

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

  var branch4 = new Pipeline<int>("b-4");
  branch4
    .AddFirstBlock(n => n)
    .AddLastBlock(n => Console.WriteLine($"Less than 10 {n}"), pipelineBlockOpts);

  var branch5 = new Pipeline<int>("b-5");
  branch5
    .AddFirstBlock(n => n)
    .AddLastBlock(n => Console.WriteLine($"Greater than 10 {n}"), pipelineBlockOpts);

  var branch1 = new Pipeline<int>("b-1");
    branch1
      .AddFirstBlock(number => number)
      .Fork()
        .Branch(n => n <= 10, branch4, pipelineBlockOpts.LinkOptions)
        .Branch(n => n > 10, branch5, pipelineBlockOpts.LinkOptions)
      ;

  var branch2 = new Pipeline<int>("b-2");
    branch2
      .AddFirstBlock(number => number)
      .AddLastBlock(n => Console.WriteLine($"Divisible by 5 {n}"), pipelineBlockOpts);

  var branch3 = new Pipeline<int>("b-3");
    branch3
      .AddFirstBlock(number => number)
      .AddLastBlock(n => Console.WriteLine($"ALL {n}"), pipelineBlockOpts);

  var pipeline = new Pipeline<string>("root");
  pipeline
    .AddFirstBlock(int.Parse)
    // .AddAsyncBlock(async n =>
    // {
    //   // Thread.Sleep(500);
    //   await Task.Delay(500);
    //   return n;
    // }, pipelineBlockOpts)
    .AddBlock(number => number*number, pipelineBlockOpts)
    // .AddLastAsyncBlock(async n => {}, pipelineBlockOpts)
    // .AddLastBlock(n => {}, pipelineBlockOpts)
    .Fork()
      .Branch(n => n % 2 == 0, branch1, pipelineBlockOpts.LinkOptions)
      .Branch(n => n % 5 == 0, branch2, pipelineBlockOpts.LinkOptions)
      .Default(branch3, pipelineBlockOpts.LinkOptions);
    ;

  var pipelineRunner = pipeline.Build();
  await pipelineRunner.ExecuteAsync(["1", "2", "3", "4", "5", "6"]);
}
