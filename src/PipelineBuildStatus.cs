namespace DataflowBuilder;

internal enum PipelineBuildStatus
{
  /// <summary>
  /// Pipeline is being built.
  /// </summary>
  Progress,
  /// <summary>
  /// Pipeline has been forked into multiple branches.
  /// </summary>
  Forked,
  /// <summary>
  /// Pipeline is ready to be built.
  /// </summary>
  ReadyForBuild,
  /// <summary>
  /// Pipeline has been built.
  /// </summary>
  Built,
}
