namespace DataflowBuilder.Runners;

/// <summary>
/// </summary>
public interface IPipelineRunner
{
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
  Task ExecuteAsync(IEnumerable<object> inputs, CancellationToken cancellationToken = default);

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
  Task ExecuteAsync(IAsyncEnumerable<object> inputs, CancellationToken cancellationToken = default);
}
