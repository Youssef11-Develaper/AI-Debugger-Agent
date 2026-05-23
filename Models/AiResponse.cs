using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AIDebuggerCli.Models
{
    public class AiResponse
    {
        private string _explanation = string.Empty;

        [JsonPropertyName("explanation")]
        public JsonElement RawExplanation { get; set; }

        [JsonPropertyName("fixedCode")]
        public string FixedCode { get; set; } = string.Empty;

        [JsonIgnore]
        public string Explanation
        {
            get
            {
                if (!string.IsNullOrEmpty(_explanation)) return _explanation;

                if (RawExplanation.ValueKind == JsonValueKind.Array)
                {
                    var sb = new StringBuilder();
                    foreach (var item in RawExplanation.EnumerateArray())
                    {
                        sb.AppendLine($"- {item.GetString()}");
                    }
                    _explanation = sb.ToString().Trim();
                }
                else if (RawExplanation.ValueKind == JsonValueKind.String)
                {
                    _explanation = RawExplanation.GetString() ?? string.Empty;
                }

                return _explanation;
            }
            set => _explanation = value;
        }
    }
}