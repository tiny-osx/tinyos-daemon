﻿using System.Diagnostics;

namespace TinyOS.Daemon.Processes; 

public class ProcessRunner {
    private List<string>? standardOutput;
    private List<string>? standardError;
    private readonly Process process;

    public ProcessRunner(FileInfo executable, ProcessArgumentBuilder? builder = null, IProcessLogger? logger = null) {
        process = new Process();
        process.StartInfo.Arguments = builder?.ToString();
        process.StartInfo.FileName = executable.FullName;
        process.StartInfo.WorkingDirectory = executable.DirectoryName;

        SetupProcessLogging(logger);
    }
    public ProcessRunner(string command, bool sudo = false, ProcessArgumentBuilder? builder = null) {
        process = new Process();
        
        var arguments = builder?.ToString();
        if (sudo == true)
        {
            process.StartInfo.FileName = $"sudo";
            process.StartInfo.Arguments = $"{command} {arguments} ";
        }
        else
        {
            process.StartInfo.FileName = command;
            process.StartInfo.Arguments = arguments;
        }

        SetupProcessLogging(null);
    }

    private void SetupProcessLogging(IProcessLogger? logger = null) {
        standardOutput = new List<string>();
        standardError = new List<string>();
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.RedirectStandardInput = true;
        process.OutputDataReceived += (s, e) => {
            if (e.Data != null) {
                if (logger != null)
                    logger.OnOutputDataReceived(e.Data);
                else standardOutput.Add(e.Data);
            }
        };
        process.ErrorDataReceived += (s, e) => {
            if (e.Data != null) {
                if (logger != null)
                    logger.OnErrorDataReceived(e.Data);
                else standardError.Add(e.Data);
            }
        };
    }

    public void SetEnvironmentVariable(string key, string value) {
        process.StartInfo.EnvironmentVariables[key] = value;
    }

    public void Kill() {
        process?.Kill();
    }
    public Process Start() {
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        return process;
    }

    public ProcessResult WaitForExit() {
        Start();
        process.WaitForExit();

        var exitCode = process.ExitCode;
        process.Close();

        return new ProcessResult(standardOutput!, standardError!, exitCode);
    }
}