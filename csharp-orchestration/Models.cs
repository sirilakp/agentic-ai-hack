using System.Text.Json.Serialization;

namespace SimpleOrchestration.Models
{
    public class ClaimOrchestrationRequest
    {
        [JsonPropertyName("claim_id")]
        public required string ClaimId { get; set; }

        [JsonPropertyName("policy_number")]
        public required string PolicyNumber { get; set; }

        [JsonPropertyName("claim_date")]
        public DateTime ClaimDate { get; set; }

        [JsonPropertyName("claim_amount")]
        public decimal ClaimAmount { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
    }

    public class ClaimOrchestrationResponse
    {
        [JsonPropertyName("claim_id")]
        public required string ClaimId { get; set; }

        [JsonPropertyName("policy_number")]
        public required string PolicyNumber { get; set; }

        [JsonPropertyName("claim_reviewer_result")]
        public string ClaimReviewerResult { get; set; } = string.Empty;

        [JsonPropertyName("risk_analyzer_result")]
        public string RiskAnalyzerResult { get; set; } = string.Empty;

        [JsonPropertyName("policy_checker_result")]
        public string PolicyCheckerResult { get; set; } = string.Empty;

        [JsonPropertyName("execution_time_ms")]
        public long ExecutionTimeMs { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("summary")]
        public string Summary { get; set; } = string.Empty;
    }
}