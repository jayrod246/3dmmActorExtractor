using Socrates.Internal;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Socrates.Compression
{
    public delegate int DecompFunk(IntPtr compressed_data, int compressed_size, [In][Out] byte[] outputbuffer, int uncompressed_size, IntPtr output_size);

    public static class Compressor
    {
        private static DecompFunk decompKCDC;
        private static DecompFunk decompKCD2;

        private static bool isInitialized = false;

        public static byte[] Decompress(byte[] input)
        {
            if (!isInitialized)
                Init();

            if (IsCompressed(input) && input.Length >= 8)
            {
                var length = input[7] | (((int)input[6]) << 8) | (((int)input[5]) << 16) | (((int)input[4]) << 24);

                if (length > 0)
                {
                    byte[] output = new byte[length];

                    int ret;
                    output = new byte[length];
                    if (length >= 0)
                    {
                        IntPtr unmanagedPointer = Marshal.AllocHGlobal(input.Length);
                        var pin = GCHandle.Alloc(-1, GCHandleType.Pinned);
                        try
                        {
                            Marshal.Copy(input, 0, unmanagedPointer, input.Length);

                            if (GetMarker(input) == "KCDC")
                            {
                                ret = decompKCDC(new IntPtr((int)unmanagedPointer + 8), input.Length - 8, output, length, pin.AddrOfPinnedObject());

                                if (ret == 1 && (int)pin.Target != -1)
                                    return output;
                            }
                            else if (GetMarker(input) == "KCD2")
                            {
                                ret = decompKCD2(new IntPtr((int)unmanagedPointer + 8), input.Length - 8, output, length, pin.AddrOfPinnedObject());

                                if (ret == 1 && (int)pin.Target != -1)
                                    return output;
                            }
                        }
                        finally
                        {
                            Marshal.FreeHGlobal(unmanagedPointer);
                            pin.Free();
                        }
                    }
                }
            }
            return input;
        }

        private static void Init()
        {
            using (var br = new System.IO.BinaryReader(new System.IO.MemoryStream(ActorExtractor.Properties.Resources.DECOMP)))
            {
                decompKCDC = NativeMethods.GetDelegateFromBytes<DecompFunk>(br.ReadBytes(br.ReadInt32()));
                decompKCD2 = NativeMethods.GetDelegateFromBytes<DecompFunk>(br.ReadBytes(br.ReadInt32()));
            }
            isInitialized = true;
        }

        public static bool IsCompressed(byte[] input)
        {
            return GetMarker(input) != null;
        }

        public static string GetMarker(byte[] input)
        {
            if (input.Length < 4)
                return null;
            var marker = Encoding.GetEncoding(1252).GetString(input, 0, 4);
            if (marker != "KCDC" && marker != "KCD2")
                return null;
            return marker;
        }
    }
}
