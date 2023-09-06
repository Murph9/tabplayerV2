using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace RevorbStd {
    public class Native {
        //required for hassle-free native lib loading on linux (without it user must have .so
        //libs installed in /libs/ or datatool path defined in LD_LIBRARY_PATH env var)
        private static IntPtr SharedLibraryResolver(string libraryName, Assembly assembly, DllImportSearchPath? p) {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return NativeLibrary.Load("../revorbstd/librevorb.dll", assembly, DllImportSearchPath.AssemblyDirectory);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return NativeLibrary.Load("../revorbstd/librevorb.so", assembly, DllImportSearchPath.AssemblyDirectory);

            Console.WriteLine("Current platform doesn't support librevorb. Sound conversion to .ogg is not available.");
            return IntPtr.Zero;
        }

        [ModuleInitializer]
        public static void LibInit() {
            NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(), SharedLibraryResolver);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct REVORB_FILE {
            public IntPtr start;
            public IntPtr cursor;
            public long size;
        }

        public const int REVORB_ERR_SUCCESS = 0;
        public const int REVORB_ERR_NOT_OGG = 1;
        public const int REVORB_ERR_FIRST_PAGE = 2;
        public const int REVORB_ERR_FIRST_PACKET = 3;
        public const int REVORB_ERR_HEADER = 4;
        public const int REVORB_ERR_TRUNCATED = 5;
        public const int REVORB_ERR_SECONDARY_HEADER = 6;
        public const int REVORB_ERR_HEADER_WRITE = 7;
        public const int REVORB_ERR_CORRUPT = 8;
        public const int REVORB_ERR_BITSTREAM_CORRUPT = 9;
        public const int REVORB_ERR_WRITE_FAIL = 10;
        public const int REVORB_ERR_WRITE_FAIL2 = 11;

        [DllImport("librevorb", CallingConvention = CallingConvention.Cdecl)]
        public static extern int revorb(ref REVORB_FILE fi, ref REVORB_FILE fo);
    }
}