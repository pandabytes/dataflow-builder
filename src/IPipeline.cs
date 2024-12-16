using DataflowBuilder.Exporters;
using DataflowBuilder.Runners;

namespace DataflowBuilder;

/// <summary>
/// Pipeline interface.
/// </summary>
public interface IPipeline
{
  /// <summary>
  /// Id of the pipeline.
  /// </summary>
  string Id { get; }

  /// <summary>
  /// Blocks in the pipeline.
  /// </summary>
  IReadOnlyList<PipelineBlock> Blocks { get; }

  /// <summary>
  /// List of branch pipelines that this
  /// pipeline has forked into.
  /// </summary>
  IReadOnlyList<IPipeline> BranchPipelines { get; }

  /// <summary>
  /// Export the pipeline into a string representation
  /// based off of <paramref name="pipelineExporter"/>.
  /// </summary>
  /// <param name="pipelineExporter">Pipeline exporter.</param>
  /// <param name="cancellationToken">Cancellation token.</param>
  /// <returns>Pipeline string representation.</returns>
  Task<string> ExportAsync(IPipelineExporter pipelineExporter, CancellationToken cancellationToken = default);

  /// <summary>
  /// Build the pipeline and return
  /// a pipeline runner object.
  /// </summary>
  internal IPipelineRunner Build();
}
