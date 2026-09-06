using System.Diagnostics;

namespace ProcessWatcher;

internal sealed class ProcessSampler
{
    private readonly Dictionary<int, (TimeSpan Cpu, DateTime Time)> _previous = new();

    public IReadOnlyList<ProcessRow> Sample()
    {
        var now = DateTime.UtcNow;
        var rows = new List<ProcessRow>();
        var alive = new HashSet<int>();

        foreach (var process in Process.GetProcesses())
        {
            using (process)
            {
                try
                {
                    int pid = process.Id;
                    alive.Add(pid);

                    string name = process.ProcessName;
                    string windowTitle = process.MainWindowTitle;
                    long memory = process.WorkingSet64;
                    TimeSpan cpu = process.TotalProcessorTime;
                    double cpuPercent = 0;

                    if (_previous.TryGetValue(pid, out var previous))
                    {
                        double elapsedMs = (now - previous.Time).TotalMilliseconds;
                        double cpuMs = (cpu - previous.Cpu).TotalMilliseconds;

                        if (elapsedMs > 0)
                        {
                            cpuPercent = cpuMs / elapsedMs / Environment.ProcessorCount * 100.0;
                            cpuPercent = Math.Clamp(cpuPercent, 0, 100);
                        }
                    }

                    _previous[pid] = (cpu, now);

                    rows.Add(new ProcessRow(
                        Name: name,
                        Pid: pid,
                        CpuPercent: cpuPercent,
                        MemoryBytes: memory,
                        Threads: process.Threads.Count,
                        WindowTitle: windowTitle));
                }
                catch
                {
                    // Protected or short-lived process.
                }
            }
        }

        foreach (int pid in _previous.Keys.Where(pid => !alive.Contains(pid)).ToArray())
            _previous.Remove(pid);

        return rows;
    }
}

internal sealed record ProcessRow(
    string Name,
    int Pid,
    double CpuPercent,
    long MemoryBytes,
    int Threads,
    string WindowTitle);
