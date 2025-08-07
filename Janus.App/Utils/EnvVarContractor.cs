using System.Collections;
using System.IO;

namespace Janus.App.Utils;

public static class EnvVarContractor
{
  public static string ContractPath(string path)
  {
    if (string.IsNullOrEmpty(path)) return path;
    string? bestVar = null;
    string? bestValue = null;
    int bestLen = 0;
    foreach (DictionaryEntry env in Environment.GetEnvironmentVariables()) {
      string var = env.Key.ToString()!;
      string value = env.Value.ToString()!;
      if (string.IsNullOrWhiteSpace(value)) continue;
      // Normalize for comparison
      string normValue = Path.GetFullPath(value).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
      string normPath = Path.GetFullPath(path);
      if (normPath.StartsWith(normValue, StringComparison.OrdinalIgnoreCase) && normValue.Length > bestLen) {
        bestVar = var;
        bestValue = normValue;
        bestLen = normValue.Length;
      }
    }
    if (bestVar != null && bestValue != null) {
      string contracted = "%" + bestVar + "%" + path.Substring(bestValue.Length - 1); // -1 to keep separator
      return contracted;
    }
    return path;
  }
}
