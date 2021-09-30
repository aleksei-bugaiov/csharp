using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;
using Newtonsoft.Json;

namespace CSharp.OverlappedIO
{
    internal static class Program
    {
        private static unsafe void Main()
        {
            const String lpFileName = "file.txt";
            SafeFileHandle fileHandle = Kernel32.CreateFile(
                lpFileName,
                (UInt32) (Kernel32.AccessRights.GENERIC_READ | Kernel32.AccessRights.GENERIC_WRITE),
                (UInt32) (Kernel32.ShareModes.FILE_SHARE_READ | Kernel32.ShareModes.FILE_SHARE_WRITE),
                IntPtr.Zero,
                (UInt32) Kernel32.CreationDispositions.OPEN_ALWAYS,
                Kernel32.FILE_FLAG_OVERLAPPED | Kernel32.FILE_FLAG_NO_BUFFERING,
                IntPtr.Zero
            );
#pragma warning disable CA1416
            ThreadPool.BindHandle(fileHandle);
            Overlapped managedOverlapped = new Overlapped();
            NativeOverlapped* nativeOverlapped = managedOverlapped.Pack(
                (code, bytes, overlapped) =>
                {
                    Console.WriteLine(
                        "-------------------------------------------\n" +
                        "Overlapped callback\n" +
                        $"Code: {JsonConvert.SerializeObject(code)}\n" +
                        $"Length: {JsonConvert.SerializeObject(bytes)}\n" +
                        $"Overlapped: {JsonConvert.SerializeObject(*overlapped)}\n" +
                        "-------------------------------------------\n"
                    );
                },
                null
            );
#pragma warning restore CA1416
            const Int32 ErrorIOPending = 997;
            const Int32 bytesToRead = 512 * 1000;
            Byte[] lpBuffer = new Byte[bytesToRead];
            Boolean result = Kernel32.ReadFile(
                fileHandle,
                lpBuffer,
                bytesToRead,
                IntPtr.Zero,
                nativeOverlapped
            );
            if (result)
            {
                Console.WriteLine(
                    "-------------------------------------------\n" +
                    "Operation completed synchronously\n" +
                    "-------------------------------------------\n"
                );
#pragma warning disable CA1416
                Overlapped.Unpack(nativeOverlapped);
                Overlapped.Free(nativeOverlapped);
#pragma warning restore CA1416
            }
            else
            {
                Int32 error = Marshal.GetLastWin32Error();
                if (ErrorIOPending != error)
                {
                    Console.WriteLine(
                        "-------------------------------------------\n" +
                        "Failed to execute DeviceIoControl using overlapped I/O\n" +
                        $"Error code: {error}\n" +
                        "-------------------------------------------\n"
                    );
#pragma warning disable CA1416
                    Overlapped.Unpack(nativeOverlapped);
                    Overlapped.Free(nativeOverlapped);
#pragma warning restore CA1416
                }
                Console.WriteLine(
                    "-------------------------------------------\n" +
                    "Finished initializing async IO\n" +
                    $"Last byte: {lpBuffer.Last()}\n" +
                    "-------------------------------------------\n"
                );
            }

            Task.Delay(10000).Wait();
        }
    }

    /// <summary>
    ///     Static imports from Kernel32.
    /// </summary>
    public static class Kernel32
    {
        [Flags]
        public enum AccessRights : UInt32
        {
            GENERIC_READ = 0x80000000,
            GENERIC_WRITE = 0x40000000,
            GENERIC_EXECUTE = 0x20000000,
            GENERIC_ALL = 0x10000000
        }

        public enum CreationDispositions
        {
            CREATE_NEW = 1,
            CREATE_ALWAYS = 2,
            OPEN_EXISTING = 3,
            OPEN_ALWAYS = 4,
            TRUNCATE_EXISTING = 5
        }

        [Flags]
        public enum ShareModes : UInt32
        {
            FILE_SHARE_READ = 0x00000001,
            FILE_SHARE_WRITE = 0x00000002,
            FILE_SHARE_DELETE = 0x00000004
        }

        public const UInt32 FILE_FLAG_OVERLAPPED = 0x40000000;
        public const UInt32 FILE_FLAG_NO_BUFFERING = 0x20000000;

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern SafeFileHandle CreateFile(
            String lpFileName,
            UInt32 dwDesiredAccess,
            UInt32 dwShareMode,
            IntPtr lpSecurityAttributes,
            UInt32 dwCreationDisposition,
            UInt32 dwFlagsAndAttributes,
            IntPtr hTemplateFile
        );

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern unsafe Boolean ReadFile(
            SafeFileHandle hFile,
            Byte[] lpBuffer,
            UInt32 nNumberOfBytesToRead,
            IntPtr lpNumberOfBytesRead,
            NativeOverlapped* lpOverlapped
        );
    }
}
