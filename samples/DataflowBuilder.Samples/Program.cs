// var linkOpts = new DataflowLinkOptions { PropagateCompletion = true };
using System.Threading.Tasks.Dataflow;
using DataflowBuilder.Exporters;
using DataflowBuilder.Runners;

var pipelineBlockOpts = new PipelineBlockOptions<ExecutionDataflowBlockOptions> { BlockOptions = new(), LinkOptions = new() { PropagateCompletion = true } };
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
  .AddLastBlock(Console.WriteLine, pipelineBlockOpts)
  // .AddLastAsyncBlock(async str =>
  // {
  //   await Task.Delay(1000);
  //   Console.WriteLine(str);
  // }, pipelineBlockOpts)
  ;

// var p = pipelineBuilder.Build();
// await p.ExecuteAsync(["1", "3", "5"]);
// await ((IPipelineRunner)p).ExecuteAsync([1,2,3]);
// await ((IPipelineRunner)p).ExecuteAsync(MyAsyncEnumerable().Select(x => (object)x));

// var batchRequests = new BatchBlock<int>(batchSize: 200);
// var sendToDb = new ActionBlock<int[]>(numbers => Console.WriteLine(string.Join(',', numbers)));

// batchRequests.LinkTo(sendToDb, new() {PropagateCompletion=true});

// await batchRequests.SendAsync(1);
// await batchRequests.SendAsync(2);
// await batchRequests.SendAsync(13);
// batchRequests.Complete();

// await sendToDb.Completion;
// System.Console.WriteLine("Done");

using var cts = new CancellationTokenSource();
pipelineBlockOpts.BlockOptions.CancellationToken = cts.Token;
cts.CancelAfter(2200);

var p = new Pipeline<int>("x");
p.AddFirstBlock(x => x, pipelineBlockOpts.BlockOptions).AddLastBlock(Console.WriteLine, pipelineBlockOpts);

var r = p.Build();
await r.ExecuteAsync(MyAsyncEnumerable());

static async IAsyncEnumerable<int> MyAsyncEnumerable()
{
    for (int i = 0; i < 50; i++)
    {
        await Task.Delay(50); // simulate some asynchronous work
        yield return i;
    }
}

static async Task FooAsync()
{
  var pipelineBlockOpts = new PipelineBlockOptions<ExecutionDataflowBlockOptions>
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
      .AddBlock(number => number)
      .Fork()
        .Branch(n => n <= 10, branch4, pipelineBlockOpts.LinkOptions)
        .Branch(n => n > 10, branch5, pipelineBlockOpts.LinkOptions)
      ;

  var branch2 = new Pipeline<int>("b-2");
    branch2
      .AddFirstBlock(number => number)
      .AddBlock(number => number.ToString())
      .AddLastBlock(n => Console.WriteLine($"Divisible by 5 {n}"), pipelineBlockOpts);

  var branch3 = new Pipeline<int>("b-3");
    branch3
      .AddFirstBlock(number => number)
      .AddLastBlock(n => Console.WriteLine($"ALL {n}"), pipelineBlockOpts);

  var pipeline = new Pipeline<string>("root");
  pipeline
    .AddFirstBlock(int.Parse)
    .AddAsyncBlock(async n =>
    {
      // Thread.Sleep(500);
      await Task.Delay(1000);
      return n;
    }, pipelineBlockOpts)
    .AddAsyncBlock(number => Task.FromResult(number), pipelineBlockOpts)
    .AddBlock(number => number*number, pipelineBlockOpts, true)
    .AddBlock(number => number, pipelineBlockOpts)
    .AddManyBlock(number =>
    {
      return Enumerable.Range(0, number);
    }, pipelineBlockOpts)
    // .Broadcast(null, pipelineBlockOpts)
    //   .Branch(branch2, pipelineBlockOpts.LinkOptions)
    //   .Branch(branch3, pipelineBlockOpts.LinkOptions)
    //   .Branch(branch4, pipelineBlockOpts.LinkOptions)
    // .AddLastAsyncBlock(async n => {}, pipelineBlockOpts)
    // .AddLastBlock(Console.WriteLine, pipelineBlockOpts)
    .Fork()
      .Branch(n => n % 2 == 0, branch1, pipelineBlockOpts.LinkOptions)
      .Branch(n => n % 5 == 0, branch2, pipelineBlockOpts.LinkOptions)
      .Default(branch3, pipelineBlockOpts.LinkOptions);
    ;

  // var pipelineRunner = pipeline.Build();
  // await pipelineRunner.ExecuteAsync(["1", "2", "3", "4", "5", "6"]);
  var g = await pipeline.ExportAsync(new GraphvizExporter());
  System.Console.WriteLine(g);
}

