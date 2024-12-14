namespace DataflowBuilder.Exporters;

/// <summary>
/// Pipeline exporter for pipeline visualization.
/// </summary>
public interface IPipelineExporter
{
  /// <summary>
  /// Export the pipeline into a string representation
  /// based off of implementation.
  /// </summary>
  /// <param name="pipeline">Pipeline to export.</param>
  /// <returns>Pipeline string representation.</returns>
  Task<string> ExportAsync(IPipeline pipeline);
}
