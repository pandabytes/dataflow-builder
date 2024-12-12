namespace DataflowBuilder.Tests;

public class PipelineTests
{
  [Theory]
  [InlineData("")]
  [InlineData(null)]

  public void Constructor_InvalidId_ThrowsException(string id)
  {
    Assert.Throws<ArgumentException>(() => new Pipeline<object>(id));
  }

  #region AddFirstBlock

  [Fact]
  public void AddFirstBlock_OutputTypeIsTask_ThrowsException()
  {
    // Arrange
    var pipeline = new Pipeline<object>("test");

    // Act & Assert
    Assert.Throws<InvalidOperationException>(() => pipeline.AddFirstBlock(x => Task.CompletedTask));
  }

  [Fact]
  public void AddFirstBlock_PipelineNotEmpty_ThrowsException()
  {
    // Arrange
    var pipeline = new Pipeline<object>("test");
    pipeline.AddFirstBlock(x => x);

    // Act & Assert
    Assert.Throws<InvalidOperationException>(() => pipeline.AddFirstBlock(x => x));
  }

  [Fact]
  public void AddFirstBlock_PipelineEmpty_BlockIsAdded()
  {
    // Arrange
    var pipeline = new Pipeline<object>("test");

    // Act
    pipeline.AddFirstBlock(x => x);
    
    // Assert
    Assert.NotNull((pipeline as IPipeline).FirstBlock);
  }

  #endregion

  #region AddFirstAyncBlock

  [Fact]
  public void AddFirstAsyncBlock_PipelineNotEmpty_ThrowsException()
  {
    // Arrange
    var pipeline = new Pipeline<object>("test");
    pipeline.AddFirstAsyncBlock(x => Task.FromResult(x));

    // Act & Assert
    Assert.Throws<InvalidOperationException>(() => pipeline.AddFirstBlock(x => x));
  }

  [Fact]
  public void AddFirstAsyncBlock_PipelineEmpty_BlockIsAdded()
  {
    // Arrange
    var pipeline = new Pipeline<object>("test");

    // Act
    pipeline.AddFirstAsyncBlock(x => Task.FromResult(x));
    
    // Assert
    Assert.NotNull((pipeline as IPipeline).FirstBlock);
  }

  #endregion

  #region AddBlock

  [Fact]
  public void AddBlock_PipelineEmpty_ThrowsException()
  {
    // Arrange
    var pipeline = new Pipeline<object>("test");

    // Act & Assert
    Assert.Throws<InvalidOperationException>(() => pipeline.AddBlock<object, object>(x => x));
  }

  [Fact]
  public void AddBlock_TaskNotAllowedAsOutputType_ThrowsException()
  {
    // Arrange
    var pipeline = new Pipeline<object>("test");
    pipeline.AddFirstBlock(x => x);

    // Act & Assert
    Assert.Throws<InvalidOperationException>(()
      => pipeline.AddBlock<object, Task<object>>(x => Task.FromResult(x), allowTaskOutput: false));
  }

  [Fact]
  public void AddBlock_TaskIsAllowedAsOutputType_BlockIsAdded()
  {
    // Arrange
    var pipeline = new Pipeline<object>("test");
    pipeline.AddFirstBlock(x => x);

    // Act
    pipeline.AddBlock<object, Task<object>>(x => Task.FromResult(x), allowTaskOutput: true);

    // Assert
    Assert.NotNull((pipeline as IPipeline).LastBlock);
  }

  [Fact]
  public void AddBlock_LastBlockIsAsync_BlockIsAdded()
  {
    // Arrange
    var pipeline = new Pipeline<string>("test");
    pipeline.AddFirstAsyncBlock(Task.FromResult);

    // Act
    pipeline.AddBlock<string, int>(int.Parse);

    // Assert
    Assert.NotNull((pipeline as IPipeline).LastBlock);
  }

  [Fact]
  public void AddBlock_LastBlockIsNotAsync_BlockIsAdded()
  {
    // Arrange
    var pipeline = new Pipeline<string>("test");
    pipeline.AddFirstBlock(x => x);

    // Act
    pipeline.AddBlock<string, int>(int.Parse);

    // Assert
    Assert.NotNull((pipeline as IPipeline).LastBlock);
  }

  #endregion

  #region AddAsyncBlock

  [Fact]
  public void AddAsyncBlock_PipelineEmpty_ThrowsException()
  {
    // Arrange
    var pipeline = new Pipeline<object>("test");

    // Act & Assert
    Assert.Throws<InvalidOperationException>(() => pipeline.AddAsyncBlock<object, int>(x => Task.FromResult(0)));
  }

  [Fact]
  public void AddAsyncBlock_LastBlockIsAsync_BlockIsAdded()
  {
    // Arrange
    var pipeline = new Pipeline<string>("test");
    pipeline.AddFirstAsyncBlock(Task.FromResult);

    // Act
    pipeline.AddAsyncBlock<string, int>(x => Task.FromResult(0));

    // Assert
    Assert.NotNull((pipeline as IPipeline).LastBlock);
  }

  [Fact]
  public void AddAsyncBlock_LastBlockIsNotAsync_BlockIsAdded()
  {
    // Arrange
    var pipeline = new Pipeline<string>("test");
    pipeline.AddFirstBlock(x => x);

    // Act
    pipeline.AddAsyncBlock<string, int>(x => Task.FromResult(0));

    // Assert
    Assert.NotNull((pipeline as IPipeline).LastBlock);
  }

  #endregion

  #region AddManyBlock

  [Fact]
  public void AddManyBlock_PipelineEmpty_ThrowsException()
  {
    // Arrange
    var pipeline = new Pipeline<object>("test");

    // Act & Assert
    Assert.Throws<InvalidOperationException>(() => pipeline.AddManyBlock<object, object>(x => []));
  }

  [Fact]
  public void AddManyBlock_PipelineNotEmpty_BlockIsAdded()
  {
    // Arrange
    var pipeline = new Pipeline<string>("test");
    pipeline.AddFirstBlock(x => x);

    // Act
    pipeline.AddManyBlock<string, char>(x => [..x]);

    // Assert
    Assert.Equal(2, pipeline.BlockCount);
  }

  #endregion

  #region AddLastBlock

  [Fact]
  public void AddLastBlock_PipelineEmpty_ThrowsException()
  {
    // Arrange
    var pipeline = new Pipeline<object>("test");

    // Act & Assert
    Assert.Throws<InvalidOperationException>(() => pipeline.AddLastBlock<object>(x => {}));
  }

  [Fact]
  public void AddLastBlock_LastBlockIsAsync_BlockIsAdded()
  {
    // Arrange
    var pipeline = new Pipeline<string>("test");
    pipeline.AddFirstAsyncBlock(Task.FromResult);

    // Act
    pipeline.AddLastBlock<string>(x => {});

    // Assert
    Assert.NotNull((pipeline as IPipeline).LastBlock);
  }

  [Fact]
  public void AddLastBlock_LastBlockIsNotAsync_BlockIsAdded()
  {
    // Arrange
    var pipeline = new Pipeline<string>("test");
    pipeline.AddFirstBlock(x => x);

    // Act
    pipeline.AddLastBlock<string>(x => {});

    // Assert
    Assert.NotNull((pipeline as IPipeline).LastBlock);
  }

  #endregion

  #region AddLastAsyncBlock

  [Fact]
  public void AddLastAsyncBlock_PipelineEmpty_ThrowsException()
  {
    // Arrange
    var pipeline = new Pipeline<object>("test");

    // Act & Assert
    Assert.Throws<InvalidOperationException>(() => pipeline.AddLastAsyncBlock<object>(x => Task.CompletedTask));
  }

  [Fact]
  public void AddLastAsyncBlock_LastBlockIsAsync_BlockIsAdded()
  {
    // Arrange
    var pipeline = new Pipeline<string>("test");
    pipeline.AddFirstAsyncBlock(Task.FromResult);

    // Act
    pipeline.AddLastAsyncBlock<string>(x => Task.CompletedTask);

    // Assert
    Assert.NotNull((pipeline as IPipeline).LastBlock);
  }

  [Fact]
  public void AddLastAsyncBlock_LastBlockIsNotAsync_BlockIsAdded()
  {
    // Arrange
    var pipeline = new Pipeline<string>("test");
    pipeline.AddFirstBlock(x => x);

    // Act
    pipeline.AddLastAsyncBlock<string>(x => Task.CompletedTask);

    // Assert
    Assert.NotNull((pipeline as IPipeline).LastBlock);
  }

  #endregion

  #region BranchOrDefault

  [Fact]
  public void BranchOrDefault_PipelineEmpty_ThrowsException()
  {
    // Arrange
    var pipeline = new Pipeline<object>("test");

    // Act & Assert
    Assert.Throws<InvalidOperationException>(()
      => pipeline.BranchOrDefault<object>(new("branch")));
  }

  [Fact]
  public void BranchOrDefault_LastBlockIsAsync_ThrowsException()
  {
    // Arrange
    var pipeline = new Pipeline<string>("test");
    pipeline.AddFirstAsyncBlock(x => Task.FromResult(0));

    var branchPipeline = new Pipeline<int>("branch");
    branchPipeline.AddFirstBlock(x => x);

    // Act & Assert
    Assert.Throws<InvalidOperationException>(() => pipeline.BranchOrDefault(branchPipeline));
  }

  [Fact]
  public void BranchOrDefault_UsePredicate_BranchPipelineIsAdded()
  {
    // Arrange
    var pipeline = new Pipeline<string>("test");
    pipeline.AddFirstBlock(x => x);

    var branchPipeline = new Pipeline<string>("branch");
    branchPipeline.AddFirstBlock(x => x);

    // Act
    pipeline.BranchOrDefault(branchPipeline, predicate: x => true);

    // Assert
    Assert.Single((pipeline as IPipeline).BranchPipelines); 
  }

  [Fact]
  public void BranchOrDefault_UseDefault_BranchPipelineIsAdded()
  {
    // Arrange
    var pipeline = new Pipeline<string>("test");
    pipeline.AddFirstBlock(x => x);

    var branchPipeline = new Pipeline<string>("branch");
    branchPipeline.AddFirstBlock(x => x);

    // Act
    pipeline.BranchOrDefault(branchPipeline, predicate: null);

    // Assert
    Assert.Single((pipeline as IPipeline).BranchPipelines); 
  }

  #endregion

  #region Broadcast

  [Fact]
  public void Broadcast_PipelineEmptyThrowsException()
  {
    // Arrange
    var pipeline = new Pipeline<string>("test");

    // Act & Assert
    Assert.Throws<InvalidOperationException>(() => pipeline.Broadcast<string>());
  }

  [Fact]
  public void Broadcast_LastBlockIsAsync_ThrowsException()
  {
    // Arrange
    var pipeline = new Pipeline<string>("test");
    pipeline.AddFirstAsyncBlock(x => Task.FromResult(0));

    // Act & Assert
    Assert.Throws<InvalidOperationException>(() => pipeline.Broadcast<string>());
  }

  [Fact]
  public void Broadcast_Valid_BroadcastBlockIsAdded()
  {
    // Arrange
    var pipeline = new Pipeline<string>("test");
    pipeline.AddFirstBlock(x => x);

    // Act
    pipeline.Broadcast<string>();

    // Assert
    Assert.Equal(2, pipeline.BlockCount);
  }

  #endregion

  #region Build

  [Fact]
  public void Build_PipelineEmpty_ThrowsException()
  {
    // Arrange
    var pipeline = new Pipeline<object>("test");

    // Act & Assert
    Assert.Throws<InvalidOperationException>(pipeline.Build);
  }

  [Fact]
  public void Build_PipelineAlreadyBuilt_ThrowsException()
  {
    // Arrange
    var pipeline = new Pipeline<object>("test");
    pipeline
      .AddFirstBlock(x => x)
      .AddLastBlock(x => {});
    pipeline.Build();

    // Act & Assert
    Assert.Throws<InvalidOperationException>(pipeline.Build);
  }

  [Fact]
  public void Build_PipelineStillInProgress_ThrowsException()
  {
    // Arrange
    var pipeline = new Pipeline<object>("test");
    pipeline.AddFirstBlock(x => x);

    // Act & Assert
    Assert.Throws<InvalidOperationException>(pipeline.Build);
  }

  [Fact]
  public void Build_PipelineForkedWithoutBranchPipeline_ThrowsException()
  {
    // Arrange
    var pipeline = new Pipeline<object>("test");
    pipeline
      .AddFirstBlock(x => x)
      .Fork();

    // Act & Assert
    Assert.Throws<InvalidOperationException>(pipeline.Build);
  }

  [Fact]
  public void Build_ValidPipeline_PipelineIsBuiltSuccessfully()
  {
    // Arrange
    var pipeline = new Pipeline<object>("test");
    pipeline
      .AddFirstBlock(x => x)
      .AddLastBlock(x => {});

    // Act & Assert
    pipeline.Build();
  }

  [Fact]
  public void Build_WithBranchPipelines_PipelineIsBuiltSuccessfully()
  {
    // Arrange
    var branchPipeline1 = new Pipeline<int>("branch-1");
    branchPipeline1
      .AddFirstBlock(x => x)
      .AddLastBlock(x => {});

    var branchPipeline2 = new Pipeline<int>("branch-2");
    branchPipeline2
      .AddFirstBlock(x => x)
      .AddLastBlock(x => {});

    var branchPipeline3 = new Pipeline<int>("branch-3");
    branchPipeline3
      .AddFirstBlock(x => x)
      .Fork()
        .Branch(x => true, branchPipeline2);

    var branchPipeline4 = new Pipeline<int>("branch-4");
    branchPipeline4
      .AddFirstBlock(x => x)
      .AddLastBlock(x => {});

    var pipeline = new Pipeline<string>("test");
    pipeline
      .AddFirstBlock(int.Parse)
      .Fork()
        .Branch(x => true, branchPipeline1)
        .Branch(x => true, branchPipeline3)
        .Default(branchPipeline4);

    // Act & Assert
    pipeline.Build();
  }

  #endregion
}
