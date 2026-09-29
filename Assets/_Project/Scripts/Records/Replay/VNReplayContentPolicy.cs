using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ProjectAllTime.VN.Records.Replay
{
    /// <summary>Explicitly approves the compiled Yarn nodes available to one Replay definition.</summary>
    public sealed class VNReplayContentPolicy
    {
        private static readonly ReadOnlyCollection<string> visualCommandNames = Array.AsReadOnly(new[]
        {
            "vn_bg",
            "vn_show",
            "vn_expression",
            "vn_move",
            "vn_facing",
            "vn_scale",
            "vn_hide",
            "vn_cg",
            "vn_clear_cg",
        });

        private readonly HashSet<string> approvedNodeNames;

        public IReadOnlyCollection<string> ApprovedNodeNames { get; }
        public static IReadOnlyCollection<string> AllowedVisualCommandNames => visualCommandNames;

        public VNReplayContentPolicy(IEnumerable<string> approvedNodeNames)
        {
            if (approvedNodeNames == null) throw new ArgumentNullException(nameof(approvedNodeNames));

            this.approvedNodeNames = new HashSet<string>(StringComparer.Ordinal);
            foreach (var nodeName in approvedNodeNames)
            {
                if (string.IsNullOrWhiteSpace(nodeName))
                    throw new ArgumentException("Replay node approvals must contain non-empty node names.", nameof(approvedNodeNames));
                this.approvedNodeNames.Add(nodeName);
            }

            ApprovedNodeNames = new ReadOnlyCollection<string>(new List<string>(this.approvedNodeNames));
        }

        public bool IsNodeApproved(string nodeName) =>
            !string.IsNullOrWhiteSpace(nodeName) && approvedNodeNames.Contains(nodeName);

        public static bool IsVisualCommandAllowed(string commandText)
        {
            if (string.IsNullOrWhiteSpace(commandText)) return false;
            var separator = commandText.IndexOfAny(new[] { ' ', '\t', '\r', '\n' });
            var commandName = separator < 0 ? commandText : commandText.Substring(0, separator);
            for (var i = 0; i < visualCommandNames.Count; i++)
            {
                if (string.Equals(visualCommandNames[i], commandName, StringComparison.Ordinal)) return true;
            }
            return false;
        }
    }
}
