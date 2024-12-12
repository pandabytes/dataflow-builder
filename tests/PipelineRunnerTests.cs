namespace DataflowBuilder.Tests;

public class PipelineRunnerTests
{
  private readonly PipelineBlockOptions _pipelineBlockOpts = new()
  {
    BlockOptions = new() { MaxDegreeOfParallelism = 1 },
    LinkOptions = new() { PropagateCompletion = true }
  };

  [Fact]
  public async Task ExecuteAsync_SimplePipeline()
  {
    // Arrange
    var results = new List<int>();
    var pipeline = new Pipeline<string>("test");
    pipeline
      .AddFirstBlock(int.Parse)
      .AddBlock(number => number * 2, _pipelineBlockOpts)
      .AddManyBlock(number => Enumerable.Range(0, number), _pipelineBlockOpts)
      .AddLastBlock(results.Add, _pipelineBlockOpts);

    // Act
    var pipelineRunner = pipeline.Build();
    await pipelineRunner.ExecuteAsync(["2"]);

    // Assert
    Assert.Equal([0, 1, 2, 3], results);
  }

  [Fact]
  public async Task ExecuteAsync_SimplePipelineWithAsync()
  {
    // Arrange
    var results = new List<int>();
    var pipeline = new Pipeline<string>("test");
    pipeline
      .AddFirstBlock(int.Parse)
      .AddBlock(number => number * 2, _pipelineBlockOpts)
      .AddAsyncBlock(async number =>
      {
        await Task.Delay(100);
        return number + 1;
      }, _pipelineBlockOpts)
      .AddLastBlock(results.Add, _pipelineBlockOpts);

    // Act
    var pipelineRunner = pipeline.Build();
    await pipelineRunner.ExecuteAsync(["1", "2", "3"]);

    // Assert
    Assert.Equal([3, 5, 7], results);
  }

  [Fact]
  public async Task ExecuteAsync_SimplePipelineWithBranches()
  {
    // Arrange
    var evenNumbers = new List<int>();
    var oddNumbers = new List<int>();

    var evenPipeline = new Pipeline<int>("even");
    evenPipeline
      .AddFirstBlock(number => number)
      .AddLastBlock(evenNumbers.Add, _pipelineBlockOpts);

    var oddPipeline = new Pipeline<int>("odd");
    oddPipeline
      .AddFirstBlock(number => number)
      .AddLastBlock(oddNumbers.Add, _pipelineBlockOpts);

    var pipeline = new Pipeline<string>("test");
    pipeline
      .AddFirstBlock(int.Parse)
      .AddAsyncBlock(async number =>
      {
        await Task.Delay(100);
        return number;
      }, _pipelineBlockOpts)
      .AddBlock(number => number + 1, _pipelineBlockOpts)
      .Fork()
        .Branch(number => number % 2 == 0, evenPipeline, _pipelineBlockOpts.LinkOptions)
        .Default(oddPipeline, _pipelineBlockOpts.LinkOptions);

    // Act
    var pipelineRunner = pipeline.Build();
    await pipelineRunner.ExecuteAsync(["1", "2", "3"]);

    // Assert
    Assert.Equal([2, 4], evenNumbers);
    Assert.Equal([3], oddNumbers);
  }

  [Fact]
  public async Task ExecuteAsync_SimplePipelineWithBroadcast()
  {
    // Arrange
    var b1Results = new List<int>();
    var b2Results = new List<int>();

    var branchPipeline1 = new Pipeline<int>("b1");
    branchPipeline1
      .AddFirstBlock(number => number)
      .AddLastBlock(b1Results.Add, _pipelineBlockOpts);

    var branchPipeline2 = new Pipeline<int>("b2");
    branchPipeline2
      .AddFirstBlock(number => number)
      .AddLastBlock(b2Results.Add, _pipelineBlockOpts);

    var pipeline = new Pipeline<string>("test");
    pipeline
      .AddFirstBlock(int.Parse)
      .AddBlock(number => number + 1, _pipelineBlockOpts)
      .Broadcast(null, _pipelineBlockOpts)
        .Branch(branchPipeline1, _pipelineBlockOpts.LinkOptions)
        .Branch(branchPipeline2, _pipelineBlockOpts.LinkOptions);

    // Act
    var pipelineRunner = pipeline.Build();
    await pipelineRunner.ExecuteAsync(["1", "2", "3"]);

    // Assert
    Assert.Equal([2, 3, 4], b1Results);
    Assert.Equal([2, 3, 4], b2Results);
  }
}
