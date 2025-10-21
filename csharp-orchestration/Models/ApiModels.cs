using System.Text.Json.Serialization;

namespace AgenticOrchestration.Models
{
    public class ClaimAnalysisRequest
    {
        [JsonPropertyName("claim_id")]
        public required string ClaimId { get; set; }

        [JsonPropertyName("policy_number")]
        public required string PolicyNumber { get; set; }

        [JsonPropertyName("requester_id")]
        public string? RequesterId { get; set; }

        [JsonPropertyName("priority")]
        public string Priority { get; set; } = "normal";
    }

    public class ClaimAnalysisResponse
    {
        [JsonPropertyName("request_id")]
        public required string RequestId { get; set; }

        [JsonPropertyName("status")]
        public required string Status { get; set; }

        [JsonPropertyName("claim_id")]
        public required string ClaimId { get; set; }

        [JsonPropertyName("policy_number")]
        public required string PolicyNumber { get; set; }

        [JsonPropertyName("analysis_summary")]
        public required AnalysisSummary AnalysisSummary { get; set; }

        [JsonPropertyName("processing_time_seconds")]
        public double ProcessingTimeSeconds { get; set; }

        [JsonPropertyName("confidence_scores")]
        public required ConfidenceScores ConfidenceScores { get; set; }

        [JsonPropertyName("agent_results")]
        public List<AgentResult> AgentResults { get; set; } = new();
    }

    public class AnalysisSummary
    {
        [JsonPropertyName("claim_status")]
        public required string ClaimStatus { get; set; }

        [JsonPropertyName("risk_level")]
        public required string RiskLevel { get; set; }

        [JsonPropertyName("coverage_decision")]
        public required string CoverageDecision { get; set; }

        [JsonPropertyName("recommendation")]
        public required string Recommendation { get; set; }

        [JsonPropertyName("agents_involved")]
        public int AgentsInvolved { get; set; }

        [JsonPropertyName("data_sources_accessed")]
        public List<string> DataSourcesAccessed { get; set; } = new();
    }

    public class ConfidenceScores
    {
        [JsonPropertyName("claim_reviewer_confidence")]
        public double ClaimReviewerConfidence { get; set; }

        [JsonPropertyName("risk_analyzer_confidence")]
        public double RiskAnalyzerConfidence { get; set; }

        [JsonPropertyName("policy_checker_confidence")]
        public double PolicyCheckerConfidence { get; set; }

        [JsonPropertyName("overall_confidence")]
        public double OverallConfidence { get; set; }
    }

    public class AgentResult
    {
        [JsonPropertyName("agent_name")]
        public required string AgentName { get; set; }

        [JsonPropertyName("agent_type")]
        public required string AgentType { get; set; }

        [JsonPropertyName("result")]
        public required string Result { get; set; }

        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }

        [JsonPropertyName("processing_time_ms")]
        public long ProcessingTimeMs { get; set; }

        [JsonPropertyName("tools_used")]
        public List<string> ToolsUsed { get; set; } = new();
    }

    public class OrchestrationRequest
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

        [JsonPropertyName("additional_data")]
        public Dictionary<string, object> AdditionalData { get; set; } = new();
    }

    public class OrchestrationResponse
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

    public class HealthResponse
    {
        [JsonPropertyName("status")]
        public required string Status { get; set; }

        [JsonPropertyName("service")]
        public required string Service { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("version")]
        public required string Version { get; set; }
    }
}