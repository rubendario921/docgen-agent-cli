using System.Diagnostics;
using System.Text;

namespace DocGen_Agent.Infrastructure.Git;

public static class GitHistoryReader
{
    public static string ReadLastChanges(int n = 10)
    {
        try
        {
            var psi = new ProcessStartInfo("git")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            psi.ArgumentList.Add("log");
            psi.ArgumentList.Add("-n");
            psi.ArgumentList.Add(n.ToString());
            psi.ArgumentList.Add("--pretty=format:| %h | %ad | %an | %s |");
            psi.ArgumentList.Add("--date=short");

            using var p = Process.Start(psi);
            if (p == null) return "_Error al iniciar git_";

            var sb = new StringBuilder();
            sb.AppendLine("| Hash | Fecha | Autor | Mensaje |");
            sb.AppendLine("| :--- | :--- | :--- | :--- |");

            string? line;
            int count = 0;
            while ((line = p.StandardOutput.ReadLine()) != null)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    sb.AppendLine(line);
                    count++;
                }
            }
            p.WaitForExit();

            if (count == 0)
                return "_Sin historial disponible_";

            return sb.ToString();
        }
        catch (Exception ex)
        {
            return $"_Sin historial disponible (Error: {ex.Message})_";
        }
    }
}