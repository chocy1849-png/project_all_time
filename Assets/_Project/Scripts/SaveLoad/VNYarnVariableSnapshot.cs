using System;
using System.Collections.Generic;
using System.Linq;
using Yarn.Unity;

namespace ProjectAllTime.VN.SaveLoad
{
    /// <summary>
    /// Captures and restores every typed Yarn variable using the plain M5 save
    /// DTO. Restore always constructs and validates complete dictionaries
    /// before making one SetAllVariables(clear: true) call.
    /// </summary>
    public static class VNYarnVariableSnapshot
    {
        public static bool TryCapture(VariableStorageBehaviour variableStorage, out YarnVariablesData snapshot, out string diagnostic)
        {
            snapshot = null;
            if (variableStorage == null)
            {
                diagnostic = "Yarn Variable Storage is required.";
                return false;
            }

            try
            {
                var allVariables = variableStorage.GetAllVariables();
                if (allVariables.FloatVariables == null || allVariables.StringVariables == null || allVariables.BoolVariables == null)
                {
                    diagnostic = "Yarn Variable Storage returned an incomplete variable set.";
                    return false;
                }

                snapshot = new YarnVariablesData
                {
                    floats = allVariables.FloatVariables
                        .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                        .Select(pair => new FloatVariableEntry { name = pair.Key, value = pair.Value })
                        .ToArray(),
                    strings = allVariables.StringVariables
                        .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                        .Select(pair => new StringVariableEntry { name = pair.Key, value = pair.Value })
                        .ToArray(),
                    bools = allVariables.BoolVariables
                        .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                        .Select(pair => new BoolVariableEntry { name = pair.Key, value = pair.Value })
                        .ToArray(),
                };

                return VNSaveSerializer.TryValidateYarnVariables(snapshot, out diagnostic);
            }
            catch (Exception)
            {
                snapshot = null;
                diagnostic = "Yarn variables could not be captured.";
                return false;
            }
        }

        public static bool TryRestore(VariableStorageBehaviour variableStorage, YarnVariablesData snapshot, out string diagnostic)
        {
            if (!TryPrepareRestore(snapshot, out var restorePlan, out diagnostic)) return false;
            return TryRestorePrepared(variableStorage, restorePlan, out diagnostic);
        }

        public static bool TryPrepareRestore(YarnVariablesData snapshot, out VNYarnVariableRestorePlan restorePlan, out string diagnostic)
        {
            restorePlan = null;
            if (!VNSaveSerializer.TryValidateYarnVariables(snapshot, out diagnostic)) return false;

            try
            {
                var floats = new Dictionary<string, float>(snapshot.floats.Length, StringComparer.Ordinal);
                var strings = new Dictionary<string, string>(snapshot.strings.Length, StringComparer.Ordinal);
                var bools = new Dictionary<string, bool>(snapshot.bools.Length, StringComparer.Ordinal);

                foreach (var entry in snapshot.floats) floats.Add(entry.name, entry.value);
                foreach (var entry in snapshot.strings) strings.Add(entry.name, entry.value);
                foreach (var entry in snapshot.bools) bools.Add(entry.name, entry.value);

                restorePlan = new VNYarnVariableRestorePlan(floats, strings, bools);
                diagnostic = null;
                return true;
            }
            catch (Exception)
            {
                diagnostic = "Saved Yarn variables could not be converted into a restore set.";
                return false;
            }
        }

        internal static bool TryRestorePrepared(VariableStorageBehaviour variableStorage, VNYarnVariableRestorePlan restorePlan, out string diagnostic)
        {
            if (variableStorage == null)
            {
                diagnostic = "Yarn Variable Storage is required.";
                return false;
            }

            if (restorePlan == null)
            {
                diagnostic = "A validated Yarn variable restore plan is required.";
                return false;
            }

            try
            {
                variableStorage.SetAllVariables(restorePlan.Floats, restorePlan.Strings, restorePlan.Bools, clear: true);
                diagnostic = null;
                return true;
            }
            catch (Exception)
            {
                diagnostic = "Saved Yarn variables could not be restored.";
                return false;
            }
        }
    }

    /// <summary>
    /// Internal immutable-by-convention transport between pre-mutation load
    /// validation and post-stop execution.
    /// </summary>
    public sealed class VNYarnVariableRestorePlan
    {
        internal Dictionary<string, float> Floats { get; }
        internal Dictionary<string, string> Strings { get; }
        internal Dictionary<string, bool> Bools { get; }

        internal VNYarnVariableRestorePlan(Dictionary<string, float> floats, Dictionary<string, string> strings, Dictionary<string, bool> bools)
        {
            Floats = floats;
            Strings = strings;
            Bools = bools;
        }
    }
}
