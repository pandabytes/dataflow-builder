# dataflow-builder

TPL Dataflow is a nice library to build pipeline to process data in memory. The downside is that
it's cumbersome to construct a pipeline that is readable and consistent. This library aims to
reduce the friction of constructing the pipeline by providing methods that is type safe and
intuitive for consumers to use.

# Usage
A simple usage can be like this:
```cs
var pipelineBlockOpts = new PipelineBlockOptions
{
  BlockOptions = new() { MaxDegreeOfParallelism = 1 },
  LinkOptions = new() { PropagateCompletion = true }
};

// <string> is the type that the pipeline accepts
var pipeline = new Pipeline<string>("test-pipeline");
pipeline
  .AddFirstBlock(int.Parse) // Convert string to int
  .AddBlock(number => number * 2, pipelineBlockOpts) // Double each int
  .AddBlock(number => number.ToString(), pipelineBlockOpts) // Convert int bakc to string
  .AddLastBlock(str => Console.WriteLine($"OUTPUT: {str}"), pipelineBlockOpts); // Print to console

var runner = pipeline.Build();
await runner.ExecuteAsync(["1", "2"]);
```

For async operation, use `AddAsyncBlock()`. You can mix both async and sync operation to
build the pipeline. But you can only have either sync or async **first** block AND either sync
or async **last** block.
```cs
var pipelineBlockOpts = new PipelineBlockOptions
{
  BlockOptions = new() { MaxDegreeOfParallelism = 1 },
  LinkOptions = new() { PropagateCompletion = true }
};

// <string> is the type that the pipeline accepts
var pipeline = new Pipeline<string>("test-pipeline");
pipeline
  .AddFirstBlock(int.Parse)
  .AddBlock(number => number * 2, pipelineBlockOpts)
  .AddAsyncBlock(async number =>
  {
    await Task.Delay(1000);
    return number.ToString();
  }, pipelineBlockOpts)
  .AddLastAsyncBlock(async str =>
  {
    await Task.Delay(1000);
    Console.WriteLine($"OUTPUT: {str}");
  }, pipelineBlockOpts);

var runner = pipeline.Build();
await runner.ExecuteAsync(["1", "2"]);
```

## Fork
This library supports "branching" off a pipeline into multiple pipelines by callling `Fork()`:
```cs
var pipelineBlockOpts = new PipelineBlockOptions
{
  BlockOptions = new() { MaxDegreeOfParallelism = 1 },
  LinkOptions = new() { PropagateCompletion = true }
};

var evenPipeline = new Pipeline<int>("even");
evenPipeline
  .AddFirstBlock(number => number)
  .AddLastBlock(number => Console.WriteLine($"EVEN: {number}"), pipelineBlockOpts);

var oddPipeline = new Pipeline<int>("odd");
oddPipeline
  .AddFirstBlock(number => number)
  .AddLastBlock(number => Console.WriteLine($"ODD: {number}"), pipelineBlockOpts);

var pipeline = new Pipeline<string>("test");
pipeline
  .AddFirstBlock(int.Parse)
  .Fork()
    // If it's even number, deliver the number to evenPipeline
    .Branch(number => number % 2 == 0, evenPipeline, pipelineBlockOpts.LinkOptions)
    // For other value, deliver to oddPipeline
    .Default(oddPipeline, pipelineBlockOpts.LinkOptions);

var runner = pipeline.Builder();
await runner.ExecuteAsync(["1", "2", "3"]);
```

## Broadcast
This library supports "broadcasting" which is delivering the same value to multiple branch pipelines by callling `Broadcast()`:
```cs
var pipelineBlockOpts = new PipelineBlockOptions
{
  BlockOptions = new() { MaxDegreeOfParallelism = 1 },
  LinkOptions = new() { PropagateCompletion = true }
};

var fbPipeline = new Pipeline<int>("fb");
fbPipeline
  .AddFirstBlock(number => number)
  .AddLastBlock(number => Console.WriteLine($"FB: {number}"), pipelineBlockOpts);

var twitterPipeline = new Pipeline<int>("twitter");
twitterPipeline
  .AddFirstBlock(number => number)
  .AddLastBlock(number => Console.WriteLine($"TWITTER: {number}"), pipelineBlockOpts);

var pipeline = new Pipeline<string>("test");
pipeline
  .AddFirstBlock(int.Parse)
  .Broadcast(null, pipelineBlockOpts)
    // For every .Branch(), the same value will be deliver to every branch pipeline
    .Branch(fbPipeline, pipelineBlockOpts.LinkOptions)
    .Branch(twitterPipeline, pipelineBlockOpts.LinkOptions);

var runner = pipeline.Builder();
await runner.ExecuteAsync(["1", "2", "3"]);
```
