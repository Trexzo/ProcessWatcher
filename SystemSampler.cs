using System.Net.NetworkInformation;

namespace ProcessWatcher;

internal sealed class SystemSampler
{
    private ulong _lastIdle;
    private ulong _lastKernel;
    private ulong _lastUser;
    private bool _cpuPrimed;

    private long _lastReceived;
    private long _lastSent;
    private DateTime _lastNetworkSample = DateTime.UtcNow;
    private bool _networkPrimed;

    public SystemSnapshot Sample()
    {
        return new SystemSnapshot(
            SampleCpu(),
            SampleMemory(),
            SampleDisk(),
            SampleNetwork(),
            TimeSpan.FromMilliseconds(Environment.TickCount64));
    }

    private double SampleCpu()
    {
        if (!NativeMethods.GetSystemTimes(out var idle, out var kernel, out var user))
            return 0;

        ulong idleNow = idle.ToUInt64();
        ulong kernelNow = kernel.ToUInt64();
        ulong userNow = user.ToUInt64();

        if (!_cpuPrimed)
        {
            _cpuPrimed = true;
            _lastIdle = idleNow;
            _lastKernel = kernelNow;
            _lastUser = userNow;
            return 0;
        }

        ulong idleDelta = idleNow - _lastIdle;
        ulong kernelDelta = kernelNow - _lastKernel;
        ulong userDelta = userNow - _lastUser;

        _lastIdle = idleNow;
        _lastKernel = kernelNow;
        _lastUser = userNow;

        ulong total = kernelDelta + userDelta;
        if (total == 0) return 0;

        double busy = total - idleDelta;
        return Math.Clamp(busy / total * 100.0, 0, 100);
    }

    private static MemorySnapshot SampleMemory()
    {
        var status = new NativeMethods.MEMORYSTATUSEX
        {
            dwLength = (uint)System.Runtime.InteropServices.Marshal.SizeOf<NativeMethods.MEMORYSTATUSEX>()
        };

        if (!NativeMethods.GlobalMemoryStatusEx(ref status))
            return new MemorySnapshot(0, 0, 0);

        ulong used = status.ullTotalPhys - status.ullAvailPhys;
        double percent = status.ullTotalPhys == 0 ? 0 : used * 100.0 / status.ullTotalPhys;
        return new MemorySnapshot(status.ullTotalPhys, used, percent);
    }

    private static DiskSnapshot SampleDisk()
    {
        try
        {
            string root = Path.GetPathRoot(Environment.SystemDirectory) ?? "C:\\";
            var drive = new DriveInfo(root);

            if (!drive.IsReady)
                return new DiskSnapshot(root, 0, 0, 0, 0);

            long total = drive.TotalSize;
            long free = drive.TotalFreeSpace;
            long used = total - free;
            double percent = total == 0 ? 0 : used * 100.0 / total;
            return new DiskSnapshot(drive.Name.TrimEnd('\\'), total, used, free, percent);
        }
        catch
        {
            return new DiskSnapshot("C:", 0, 0, 0, 0);
        }
    }

    private NetworkSnapshot SampleNetwork()
    {
        long received = 0;
        long sent = 0;

        foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (nic.OperationalStatus != OperationalStatus.Up ||
                nic.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                continue;

            try
            {
                var stats = nic.GetIPv4Statistics();
                received += stats.BytesReceived;
                sent += stats.BytesSent;
            }
            catch
            {
                // Adapter changed while sampling.
            }
        }

        var now = DateTime.UtcNow;
        double seconds = Math.Max((now - _lastNetworkSample).TotalSeconds, 0.001);

        if (!_networkPrimed)
        {
            _networkPrimed = true;
            _lastReceived = received;
            _lastSent = sent;
            _lastNetworkSample = now;
            return new NetworkSnapshot(0, 0);
        }

        long recvDelta = Math.Max(0, received - _lastReceived);
        long sendDelta = Math.Max(0, sent - _lastSent);

        _lastReceived = received;
        _lastSent = sent;
        _lastNetworkSample = now;

        return new NetworkSnapshot(recvDelta / seconds, sendDelta / seconds);
    }
}

internal sealed record SystemSnapshot(
    double CpuPercent,
    MemorySnapshot Memory,
    DiskSnapshot Disk,
    NetworkSnapshot Network,
    TimeSpan Uptime);

internal sealed record MemorySnapshot(ulong TotalBytes, ulong UsedBytes, double Percent);
internal sealed record DiskSnapshot(string Drive, long TotalBytes, long UsedBytes, long FreeBytes, double Percent);
internal sealed record NetworkSnapshot(double BytesReceivedPerSecond, double BytesSentPerSecond);
