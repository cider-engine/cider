using Microsoft.IO;
using SDL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cider.Platform.Browser
{
    [SupportedOSPlatform("browser")]
    public static class Browser
    {
        private static int IOStreamUnderlyingStreamId = int.MinValue;
#nullable enable
        private static readonly Dictionary<int, RecyclableMemoryStream> IOStreamUnderlyingStreams = new();

        private static readonly RecyclableMemoryStreamManager MemoryStreamManagaer = new();

        public static readonly HttpClient Client;

        public static string LocationHref { get; private set; }

        static Browser()
        {
            if (!OperatingSystem.IsBrowser()) throw new PlatformNotSupportedException();

            Client = new();

            using var location = JSHost.GlobalThis.GetPropertyAsJSObject("location");
            LocationHref = location!.GetPropertyAsString("href")!;
        }

        internal static async Task<(SDL_IOStreamInterface context, int id)> HttpResponseToIOStreamInterface(HttpResponseMessage response, CancellationToken token)
        {
            SDL3.SDL_INIT_INTERFACE(out SDL_IOStreamInterface context);

            unsafe
            {
                context.size = &Size;
                context.seek = &Seek;
                context.read = &Read;
                context.write = &Write;
                context.flush = &Flush;
                context.close = &Close;
            }

            var id = IOStreamUnderlyingStreamId++; // 浏览器环境不需要使用Interlocked.Increment

            var memoryStream = MemoryStreamManagaer.GetStream();

            using var contentStream = await response.Content.ReadAsStreamAsync(token);

            await contentStream.CopyToAsync(memoryStream, token);

            memoryStream.Position = 0;

            IOStreamUnderlyingStreams.Add(id, memoryStream);

            return (context, id);

            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
            static unsafe long Size(nint userdata)
            {
                try
                {
                    var id = *((int*)userdata);

                    var stream = IOStreamUnderlyingStreams[id];

                    return stream.Length;
                }
                catch (Exception)
                {
                    Debug.Assert(false);
                    return -1;
                }
            }

            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
            static unsafe long Seek(nint userdata, long offset, SDL_IOWhence whence)
            {
                try
                {
                    var id = *((int*)userdata);

                    var stream = IOStreamUnderlyingStreams[id];

                    Debug.Assert(stream.CanSeek);

                    return stream.Seek(offset, whence switch
                    {
                        SDL_IOWhence.SDL_IO_SEEK_SET => SeekOrigin.Begin,
                        SDL_IOWhence.SDL_IO_SEEK_CUR => SeekOrigin.Current,
                        SDL_IOWhence.SDL_IO_SEEK_END => SeekOrigin.End,
                        _ => throw new CiderGameException()
                    });
                }
                catch (Exception)
                {
                    Debug.Assert(false);
                    return -1;
                }
            }

            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
            static unsafe nuint Read(nint userdata, nint ptr, nuint size, SDL_IOStatus* status)
            {
                try
                {
                    var id = *((int*)userdata);

                    var stream = IOStreamUnderlyingStreams[id];

                    Debug.Assert(stream.CanRead);

                    var span = new Span<byte>((void*)ptr, (int)size);

                    var number = (nuint)stream.Read(span);

                    return number;
                }
                catch (Exception)
                {
                    *status = SDL_IOStatus.SDL_IO_STATUS_ERROR;
                    Debug.Assert(false);
                    return 0;
                }
            }

            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
            static unsafe nuint Write(nint userdata, nint ptr, nuint size, SDL_IOStatus* status)
            {
                try
                {
                    var id = *((int*)userdata);

                    var stream = IOStreamUnderlyingStreams[id];

                    Debug.Assert(stream.CanWrite);

                    var span = new Span<byte>((void*)ptr, (int)size);

                    stream.Write(span);

                    return size;
                }
                catch (Exception)
                {
                    *status = SDL_IOStatus.SDL_IO_STATUS_ERROR;
                    Debug.Assert(false);
                    return 0;
                }
            }

            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
            static unsafe SDLBool Flush(nint userdata, SDL_IOStatus * status)
            {
                try
                {
                    var id = *((int*)userdata);

                    var stream = IOStreamUnderlyingStreams[id];

                    stream.Flush();

                    return true;
                }
                catch (Exception)
                {
                    *status = SDL_IOStatus.SDL_IO_STATUS_ERROR;
                    Debug.Assert(false);
                    return false;
                }
            }

            [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
            static unsafe SDLBool Close(nint userdata)
            {
                try
                {
                    var id = *((int*)userdata);

                    var stream = IOStreamUnderlyingStreams[id];

                    stream.Close();

                    IOStreamUnderlyingStreams.Remove(id);

                    return true;
                }
                catch (Exception)
                {
                    Debug.Assert(false);
                    return false;
                }
            }
        }
    }
}
