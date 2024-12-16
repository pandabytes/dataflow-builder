namespace DataflowBuilder.Runners;

/// <summary>
/// Base class for pipeline runner.
/// </summary>
/// <typeparam name="TIn">Type to feed to pipeline runner.</typeparam>
public abstract class BasePipelineRunner<TIn> : IPipelineRunner
{
  /// <inheritdoc/>
  async Task IPipelineRunner.ExecuteAsync(IEnumerable<object> inputs, CancellationToken cancellationToken)
  {
    try
    {
      var castInputs = inputs.Cast<TIn>();
      await ExecuteAsync(castInputs, cancellationToken).NoState();
    }
    catch (InvalidCastException ex)
    {
      throw new ArgumentException($"Expected input to be of type {typeof(TIn).FullName}.", ex);
    }
  }

  /// <inheritdoc/>
  async Task IPipelineRunner.ExecuteAsync(IAsyncEnumerable<object> inputs, CancellationToken cancellationToken)
  {
    try
    {
      var castInputs = inputs.Cast<TIn>();
      await ExecuteAsync(castInputs, cancellationToken).NoState();
    }
    catch (InvalidCastException ex)
    {
      throw new ArgumentException($"Expected input to be of type {typeof(TIn).FullName}.", ex);
    }
  }

  /// <summary>
  /// Execute the pipeline with <paramref name="inputs"/>.
  /// </summary>
  /// <param name="inputs">Inputs to feed to pipeline.</param>
  /// <param name="cancellationToken">
  /// Cancellation token for canceling sending input to pipeline, NOT to cancel the block(s) in the pipeline.
  /// </param>
  /// <exception cref="InvalidOperationException">
  /// A pipeline can only run once, it run again it will throw an exception.
  /// </exception>
  public abstract Task ExecuteAsync(IEnumerable<TIn> inputs, CancellationToken cancellationToken = default);

  /// <summary>
  /// Execute the pipeline with <paramref name="inputs"/>.
  /// </summary>
  /// <param name="inputs">Inputs to feed to pipeline.</param>
  /// <param name="cancellationToken">
  /// Cancellation token for canceling sending input to pipeline, NOT to cancel the block(s) in the pipeline.
  /// </param>
  /// <exception cref="InvalidOperationException">
  /// A pipeline can only run once, it run again it will throw an exception.
  /// </exception>
  public abstract Task ExecuteAsync(IAsyncEnumerable<TIn> inputs, CancellationToken cancellationToken = default);
}
