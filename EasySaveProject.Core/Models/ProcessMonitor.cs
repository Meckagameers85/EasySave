using System.Diagnostics;

namespace EasySaveProject.Core.Models;

public class ProcessMonitor
{
    private readonly string _processNameToWatch;
    private readonly bool _isEnabled;

    public ProcessMonitor(string processName)
    {
        /*
            Visibility : public
            Input : string processName
            Output : None
            Description : Constructor that initializes the process monitor with the name of the process to watch.
        */
        _processNameToWatch = processName?.Trim() ?? "";
        _isEnabled = !string.IsNullOrEmpty(_processNameToWatch);
    }

    public bool IsBusinessSoftwareRunning()
    {
        /*
            Visibility : public
            Input : None
            Output : bool
            Description : Checks if the business software is currently running. Returns false if monitoring is disabled.
        */
        if (!_isEnabled) return false;

        try
        {
            // Recherche par nom de processus (sans extension .exe)
            var processName = _processNameToWatch.Replace(".exe", "");
            var processes = Process.GetProcessesByName(processName);
            
            return processes.Length > 0;
        }
        catch (Exception)
        {
            // En cas d'erreur (permissions, etc.), on considère que le logiciel ne tourne pas
            return false;
        }
    }

    public List<string> GetRunningBusinessSoftwareProcesses()
    {
        /*
            Visibility : public
            Input : None
            Output : List<string>
            Description : Returns a list of all running processes matching the business software name.
        */
        var runningProcesses = new List<string>();
        
        if (!_isEnabled) return runningProcesses;

        try
        {
            var processName = _processNameToWatch.Replace(".exe", "");
            var processes = Process.GetProcessesByName(processName);
            
            foreach (var process in processes)
            {
                try
                {
                    runningProcesses.Add($"{process.ProcessName} (PID: {process.Id})");
                }
                catch
                {
                    // Ignorer les processus auxquels on n'a pas accès
                }
            }
        }
        catch (Exception)
        {
            // En cas d'erreur globale, retourner une liste vide
        }

        return runningProcesses;
    }

    public string GetProcessNameToWatch()
    {
        /*
            Visibility : public
            Input : None
            Output : string
            Description : Returns the name of the process being monitored.
        */
        return _processNameToWatch;
    }

    public bool IsMonitoringEnabled()
    {
        /*
            Visibility : public
            Input : None
            Output : bool
            Description : Returns true if process monitoring is enabled (i.e., a process name was provided).
        */
        return _isEnabled;
    }
}