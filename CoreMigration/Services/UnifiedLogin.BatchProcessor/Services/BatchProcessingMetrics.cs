using System.Diagnostics.Metrics;

namespace UnifiedLogin.BatchProcessor.Services
{
    // <summary>
    /// Metrics collector for batch processing operations.
    /// Uses .NET's built-in System.Diagnostics.Metrics for OpenTelemetry integration.
    /// </summary>
    public class BatchProcessingMetrics
    {
        private readonly Meter _meter;
        private readonly Counter<long> _batchesProcessedCounter;
        private readonly Counter<long> _batchesFailedCounter;
        private readonly Histogram<double> _processingDurationHistogram;
        private readonly Counter<long> _totalBatchesRetrievedCounter;

        public BatchProcessingMetrics(IMeterFactory meterFactory)
        {
            _meter = meterFactory.Create("UnifiedLogin.BatchProcessor");

            _batchesProcessedCounter = _meter.CreateCounter<long>(
                name: "batch.processed.count",
                unit: "batches",
                description: "Total number of batches processed successfully");

            _batchesFailedCounter = _meter.CreateCounter<long>(
                name: "batch.failed.count",
                unit: "batches",
                description: "Total number of batches that failed processing");

            _processingDurationHistogram = _meter.CreateHistogram<double>(
                name: "batch.processing.duration",
                unit: "ms",
                description: "Duration of batch processing cycle in milliseconds");

            _totalBatchesRetrievedCounter = _meter.CreateCounter<long>(
                name: "batch.retrieved.count",
                unit: "batches",
                description: "Total number of batches retrieved from database");
        }

        public void RecordBatchesRetrieved(string jobName, int count)
        {
            _totalBatchesRetrievedCounter.Add(count,
                new KeyValuePair<string, object?>("job.name", jobName));
        }

        public void RecordBatchProcessed(string jobName, int successCount, int failureCount, double durationMs)
        {
            var tags = new KeyValuePair<string, object?>("job.name", jobName);

            if (successCount > 0)
            {
                _batchesProcessedCounter.Add(successCount, tags);
            }

            if (failureCount > 0)
            {
                _batchesFailedCounter.Add(failureCount, tags);
            }

            _processingDurationHistogram.Record(durationMs, tags);
        }
    }
}
