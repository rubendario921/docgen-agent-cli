using System.Diagnostics;
using System.Text;

namespace DocGen_Agent.Infrastructure.Git;

public static class GitHistoryReader
{
    public static string ReadLastChanges(int n = 10)
    {
        try
        {
            var psi = new ProcessStartInfo("git", $"log -n {n} --pretty=\"format:| %h | %ad | %an | %s |\" --date=short")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            var p = Process.Start(psi)!;
            var output = p.StandardOutput.ReadToEnd().Trim();
            p.WaitForExit();

            if (string.IsNullOrWhiteSpace(output))
                return "_Sin historial disponible_";

            var sb = new StringBuilder();
            sb.AppendLine("| Hash | Fecha | Autor | Mensaje |");
            sb.AppendLine("| :--- | :--- | :--- | :--- |");
            sb.AppendLine(output);

            return sb.ToString();
        }
        catch
        {
            return "_Sin historial disponible_";
        }
    }
}