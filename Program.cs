using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CacheSize
{
    internal class Program
    {
        static void Help()
        {
            string HelpStr = "CacheSize [-get] [-setmax dd] [-reset]" + Environment.NewLine +
                 "  -get              Print current file system cache size settings." + Environment.NewLine +
                 "  -setmax dd        Set file system cache size to dd MB. May fail if value is too small." + Environment.NewLine + 
                 "  -reset            Disable hard cache size size limit." + Environment.NewLine;
            Console.WriteLine(HelpStr);
        }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Help();
                return;
            }

            string cmd = args[0].ToLowerInvariant();

            switch (cmd)
            {
                case "-get":
                    GetCacheSize();
                    break;
                case "-setmax":
                    Privileges.EnablePrivilege(SecurityEntity.SE_INCREASE_QUOTA_NAME); // to set we need the privilege SE_INCREASE_QUOTA_NAME
                    if (args.Length < 2)
                    {
                        Console.WriteLine("You need to enter a maximum file cache size in MB.");
                        return;
                    }
                    long newMaxMB = long.Parse(args[1]);
                    IntPtr newMax = new IntPtr(newMaxMB * 1024L * 1024L);
                    Console.WriteLine($"Set Max Hard Max: {newMax.ToInt64() / 1024:N0} KB");
                    int iret = SetSystemFileCacheSize(IntPtr.Zero, newMax, File_Cache_Flags.MIN_HARD_DISABLE | File_Cache_Flags.MAX_HARD_ENABLE);
                    if (iret == 0)
                    {
                        throw new Win32Exception();
                    }
                    break;
                case "-reset":
                    Privileges.EnablePrivilege(SecurityEntity.SE_INCREASE_QUOTA_NAME); // to set we need the privilege SE_INCREASE_QUOTA_NAME
                    Console.WriteLine("Resetting hard limits");
                    iret = SetSystemFileCacheSize(IntPtr.Zero, IntPtr.Zero, File_Cache_Flags.MIN_HARD_DISABLE | File_Cache_Flags.MAX_HARD_DISABLE);
                    if (iret == 0)
                    {
                        throw new Win32Exception();
                    }
                    break;
                default:
                    Help();
                    Console.WriteLine($"Error: Invalid command: {cmd}");
                    break;
            }
        }

        /// <summary>Flags for use with SetSystemFileCacheSize.  Note that corresponding enable & disable are mutually exclusive and will fail.</summary>
        [Flags]
        public enum File_Cache_Flags : uint // 32 bits
        {
            None = 0,
            MAX_HARD_ENABLE = 0x00000001,
            MAX_HARD_DISABLE = 0x00000002,
            MIN_HARD_ENABLE = 0x00000004,
            MIN_HARD_DISABLE = 0x00000008,
        }

        [DllImport("kernel32.dll", SetLastError = true)]

        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetSystemFileCacheSize(ref IntPtr lpMinimumFileCacheSize, ref IntPtr lpMaximumFileCacheSize, ref File_Cache_Flags Flags);

        /// <summary>Limits the size of the working set for the file system cache.</summary>
        /// <param name="MinimumFileCacheSize">The minimum size of the file cache, in bytes. To flush, use UInt64.MaxValue.</param>
        /// <param name="MaximumFileCacheSize">The maximum size of the file cache, in bytes. Must be > min + 64KB</param>
        /// <param name="Flags">See File_Cache_Flags</param>
        /// <returns>0=fail !0=success</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern Int32 SetSystemFileCacheSize(IntPtr MinimumFileCacheSize, IntPtr MaximumFileCacheSize, File_Cache_Flags Flags);

        static (long min, long max, File_Cache_Flags flags) GetCacheSize()
        {
            IntPtr minCacheSize = default;
            IntPtr maxCacheSize = default;
            File_Cache_Flags flags = default;

            bool lret = GetSystemFileCacheSize(ref minCacheSize, ref maxCacheSize, ref flags);
            if (!lret)
            {
                throw new Win32Exception();
            }

            PerformanceCounter counter = new PerformanceCounter("Memory", "Cache Bytes");
            long cacheSize = counter.NextSample().RawValue;


            Console.WriteLine($"Cache Size Min:  {(minCacheSize.ToInt64() / 1024).ToString("N0"),15} KB");
            Console.WriteLine($"Cache Size Max:  {(maxCacheSize.ToInt64() / 1024).ToString("N0"),15} KB");
            Console.WriteLine($"Current Size  :  {(cacheSize / 1024).ToString("N0"),15} KB (according to Performance Counter (Memory/Cache Bytes))");
            Console.WriteLine($"Cache Flags   :  {flags,15}");

            return (minCacheSize.ToInt64(), maxCacheSize.ToInt64(), flags);
        }
    }
}
