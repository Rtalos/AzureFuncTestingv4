using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1
{
    public static class FunctionHostHelper
    {
        //POC 1
        public static void StartHostProcess(string functionName)
        {

            var process = new Process
            {
                StartInfo =
                {
                    FileName = "powershell.exe",
                    Arguments = @"$func = $(Get-Command func).Source.Replace("".ps1"", "".cmd""); Start-Process -NoNewWindow ""$func"" @(""start"",""--verbose"",""false"")",
                    WorkingDirectory = GetFunctionPath(functionName)
                }
            };
            process.Start();
        }

        //POC 2 from azure func github e2e tests
        public static void StartFuncHostProcess(string functionName)
        {
            var funcProcess = new Process();
            var functionFolder = GetFunctionPath(functionName);
            string e2eHostJson = Directory.GetFiles(functionFolder, "host.json", SearchOption.AllDirectories).FirstOrDefault();

            if (e2eHostJson == null)
            {
                throw new InvalidOperationException($"No existing host file. Make sure FunctionName is correct. '{functionFolder}'");
            }

            //TODO not ze best
            string executable = System.IO.Directory.GetFiles(
                        "C:\\Program Files\\Microsoft\\Azure Functions Core Tools",
                        "func.exe", System.IO.SearchOption.AllDirectories)[0];

            if (!File.Exists(executable))
            {
                throw new InvalidOperationException($"Azure Func CLI not found on this path '{executable}'.");
            }

            //funcProcess.StartInfo.UseShellExecute = true;
            //funcProcess.StartInfo.RedirectStandardError = false;
            //funcProcess.StartInfo.RedirectStandardOutput = false;
            funcProcess.StartInfo.CreateNoWindow = true;
            funcProcess.StartInfo.WorkingDirectory = functionFolder;
            funcProcess.StartInfo.FileName = executable;
            funcProcess.StartInfo.ArgumentList.Add("host");
            funcProcess.StartInfo.ArgumentList.Add("start");
            funcProcess.StartInfo.ArgumentList.Add("--verbose");

            funcProcess.Start();
        }

        public static void KillFuncHosts()
        {
            foreach (var func in Process.GetProcessesByName("func"))
            {
                try
                {
                    func.Kill();
                }
                catch
                {
                    // gg qq wp
                }
            }
        }

        public static string GetAssemblyDir() => Path.GetDirectoryName(GetAssemblyPath());
        public static string GetAssemblyPath() => Assembly.GetExecutingAssembly().Location;
        public static string GetSolutionDir() => Directory.GetParent(GetSolutionPath()).FullName;
        public static string GetFunctionPath(string functionName) => $"{Directory.GetParent(GetSolutionPath()).FullName}\\{functionName}";
        public static string GetSolutionPath()
        {
            var currentDirPath = GetAssemblyDir();
            while (currentDirPath != null)
            {
                var fileInCurrentDir = Directory.GetFiles(currentDirPath).Select(f => f.Split(@"\").Last()).ToArray();
                var solutionFileName = fileInCurrentDir.SingleOrDefault(f => f.EndsWith(".sln", StringComparison.InvariantCultureIgnoreCase));
                if (solutionFileName != null)
                    return Path.Combine(currentDirPath, solutionFileName);

                currentDirPath = Directory.GetParent(currentDirPath)?.FullName;
            }

            throw new FileNotFoundException("Cannot find solution file path");
        }
    }
}
