﻿using SubtitleEdit.Logic;
using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers
{
    public class LibMpvDynamic : IDisposable
    {

        #region LibMpv methods - see https://github.com/mpv-player/mpv/blob/master/libmpv/client.h

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr MpvCreate();
        private MpvCreate _mpvCreate;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int MpvInitialize(IntPtr mpvHandle);
        private MpvInitialize _mpvInitialize;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int MpvCommand(IntPtr mpvHandle, IntPtr utf8Strings);
        private MpvCommand _mpvCommand;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr MpvWaitEvent(IntPtr mpvHandle, double wait);
        private MpvWaitEvent _mpvWaitEvent;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int MpvSetOption(IntPtr mpvHandle, byte[] name, int format, ref long data);
        private MpvSetOption _mpvSetOption;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int MpvSetOptionString(IntPtr mpvHandle, byte[] name, byte[] value);
        private MpvSetOptionString _mpvSetOptionString;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int MpvGetPropertyString(IntPtr mpvHandle, byte[] name, int format, ref IntPtr data);
        private MpvGetPropertyString _mpvGetPropertyString;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int MpvGetPropertyDouble(IntPtr mpvHandle, byte[] name, int format, ref double data);
        private MpvGetPropertyDouble _mpvGetPropertyDouble;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int MpvSetProperty(IntPtr mpvHandle, byte[] name, int format, ref byte[] data);
        private MpvSetProperty _mpvSetProperty;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void MpvFree(IntPtr data);
        private MpvFree _mpvFree;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate ulong MpvClientApiVersion();
        private MpvClientApiVersion _mpvClientApiVersion;

        #endregion

        private static IntPtr _libMpvDll = IntPtr.Zero;
        private IntPtr _mpvHandle;
        private Timer _videoLoadedTimer;
        private double? _pausePosition; // Hack to hold precise seeking when paused
        private string _secondSubtitleFileName;
        private int _brightness; // -100 to 100
        private int _contrast; // -100 to 100

        public event EventHandler OnVideoLoaded;
        public event EventHandler OnVideoEnded;

        private const int MpvFormatString = 1;

        private object GetDllType(Type type, string name)
        {
            var address = NativeMethods.CrossGetProcAddress(_libMpvDll, name);
            return address != IntPtr.Zero ? Marshal.GetDelegateForFunctionPointer(address, type) : null;
        }

        private void LoadLibMpvDynamic()
        {
            _mpvCreate = (MpvCreate)GetDllType(typeof(MpvCreate), "mpv_create");
            _mpvInitialize = (MpvInitialize)GetDllType(typeof(MpvInitialize), "mpv_initialize");
            _mpvWaitEvent = (MpvWaitEvent)GetDllType(typeof(MpvWaitEvent), "mpv_wait_event");
            _mpvCommand = (MpvCommand)GetDllType(typeof(MpvCommand), "mpv_command");
            _mpvSetOption = (MpvSetOption)GetDllType(typeof(MpvSetOption), "mpv_set_option");
            _mpvSetOptionString = (MpvSetOptionString)GetDllType(typeof(MpvSetOptionString), "mpv_set_option_string");
            _mpvGetPropertyString = (MpvGetPropertyString)GetDllType(typeof(MpvGetPropertyString), "mpv_get_property");
            _mpvGetPropertyDouble = (MpvGetPropertyDouble)GetDllType(typeof(MpvGetPropertyDouble), "mpv_get_property");
            _mpvSetProperty = (MpvSetProperty)GetDllType(typeof(MpvSetProperty), "mpv_set_property");
            _mpvFree = (MpvFree)GetDllType(typeof(MpvFree), "mpv_free");
            _mpvClientApiVersion = (MpvClientApiVersion)GetDllType(typeof(MpvClientApiVersion), "mpv_client_api_version");
        }

        private bool IsAllMethodsLoaded()
        {
            return _mpvCreate != null &&
                   _mpvInitialize != null &&
                   _mpvWaitEvent != null &&
                   _mpvCommand != null &&
                   _mpvSetOption != null &&
                   _mpvSetOptionString != null &&
                   _mpvGetPropertyString != null &&
                   _mpvGetPropertyDouble != null &&
                   _mpvSetProperty != null &&
                   _mpvFree != null;
        }

        private static byte[] GetUtf8Bytes(string s)
        {
            return Encoding.UTF8.GetBytes(s + "\0");
        }

        public static IntPtr AllocateUtf8IntPtrArrayWithSentinel(string[] arr, out IntPtr[] byteArrayPointers)
        {
            int numberOfStrings = arr.Length + 1; // add extra element for extra null pointer last (sentinel)
            byteArrayPointers = new IntPtr[numberOfStrings];
            IntPtr rootPointer = Marshal.AllocCoTaskMem(IntPtr.Size * numberOfStrings);
            for (int index = 0; index < arr.Length; index++)
            {
                var bytes = GetUtf8Bytes(arr[index]);
                IntPtr unmanagedPointer = Marshal.AllocHGlobal(bytes.Length);
                Marshal.Copy(bytes, 0, unmanagedPointer, bytes.Length);
                byteArrayPointers[index] = unmanagedPointer;
            }
            Marshal.Copy(byteArrayPointers, 0, rootPointer, numberOfStrings);
            return rootPointer;
        }

        private void DoMpvCommand(params string[] args)
        {
            if (_mpvHandle == IntPtr.Zero)
            {
                return;
            }

            var mainPtr = AllocateUtf8IntPtrArrayWithSentinel(args, out var byteArrayPointers);
            _mpvCommand(_mpvHandle, mainPtr);
            foreach (var ptr in byteArrayPointers)
            {
                Marshal.FreeHGlobal(ptr);
            }
            Marshal.FreeHGlobal(mainPtr);
        }

        public string PlayerName => $"libmpv {VersionNumber}";

        private int _volume = 75;
        public int Volume
        {
            get => _volume;
            set
            {
                DoMpvCommand("set", "volume", value.ToString(CultureInfo.InvariantCulture));
                _volume = value;
            }
        }

        public double Duration
        {
            get
            {
                lock (_lockObj)
                {
                    if (_mpvHandle == IntPtr.Zero)
                    {
                        return 0;
                    }

                    int mpvFormatDouble = 5;
                    double d = 0;
                    _mpvGetPropertyDouble(_mpvHandle, GetUtf8Bytes("duration"), mpvFormatDouble, ref d);
                    return d;
                }
            }
        }

        public double CurrentPosition
        {
            get
            {
                lock (_lockObj)
                {
                    if (_mpvHandle == IntPtr.Zero)
                    {
                        return 0;
                    }

                    if (_pausePosition != null)
                    {
                        if (_pausePosition < 0)
                        {
                            return 0;
                        }

                        return _pausePosition.Value;
                    }

                    int mpvFormatDouble = 5;
                    double d = 0;
                    _mpvGetPropertyDouble(_mpvHandle, GetUtf8Bytes("time-pos"), mpvFormatDouble, ref d);
                    return d;
                }
            }
            set
            {
                lock (_lockObj)
                {

                    if (_mpvHandle == IntPtr.Zero)
                    {
                        return;
                    }

                    if (IsPaused && value <= Duration)
                    {
                        _pausePosition = value;
                    }

                    DoMpvCommand("seek", value.ToString(CultureInfo.InvariantCulture), "absolute");
                }
            }
        }

        private double _playRate = 1.0;
        public double PlayRate
        {
            get => _playRate;
            set
            {
                DoMpvCommand("set", "speed", value.ToString(CultureInfo.InvariantCulture));
                _playRate = value;
            }
        }

        public void GetNextFrame()
        {
            if (_mpvHandle == IntPtr.Zero)
            {
                return;
            }

            DoMpvCommand("frame-step");
            _pausePosition = null;
        }

        public void GetPreviousFrame()
        {
            if (_mpvHandle == IntPtr.Zero)
            {
                return;
            }

            DoMpvCommand("frame-back-step");
            _pausePosition = null;
        }

        public void Play()
        {
            lock (_lockObj)
            {
                if (_mpvHandle == IntPtr.Zero)
                {
                    return;
                }

                _pausePosition = null;

                if (_mpvHandle == IntPtr.Zero)
                {
                    return;
                }

                var bytes = GetUtf8Bytes("no");
                _mpvSetProperty(_mpvHandle, GetUtf8Bytes("pause"), MpvFormatString, ref bytes);
            }
        }

        public void Pause()
        {
            lock (_lockObj)
            {
                if (_mpvHandle == IntPtr.Zero)
                {
                    return;
                }

                var bytes = GetUtf8Bytes("yes");
                _mpvSetProperty(_mpvHandle, GetUtf8Bytes("pause"), MpvFormatString, ref bytes);
            }
        }

        public void Stop()
        {
            lock (_lockObj)
            {

                if (_mpvHandle == IntPtr.Zero)
                {
                    return;
                }

                Pause();
                _pausePosition = null;
                CurrentPosition = 0;
            }
        }

        public bool IsPaused
        {
            get
            {
                if (_mpvHandle == IntPtr.Zero)
                {
                    return true;
                }

                var lpBuffer = IntPtr.Zero;
                _mpvGetPropertyString(_mpvHandle, GetUtf8Bytes("pause"), MpvFormatString, ref lpBuffer);
                var isPaused = Marshal.PtrToStringAnsi(lpBuffer) == "yes";
                _mpvFree(lpBuffer);
                return isPaused;
            }
        }

        public bool IsPlaying => !IsPaused;

        public string VersionNumber
        {
            get
            {
                if (_mpvHandle == IntPtr.Zero)
                {
                    return string.Empty;
                }

                var version = _mpvClientApiVersion();
                var high = version >> 16;
                var low = version & 0xff;
                return high + "." + low;
            }
        }

        public void LoadSubtitle(string fileName)
        {
            DoMpvCommand("sub-add", fileName, "select");
        }

        public void LoadSecondSubtitle(string fileName)
        {
            _secondSubtitleFileName = fileName;
            DoMpvCommand("sub-add", fileName, "select");
        }

        public void RemoveSubtitle()
        {
            if (!string.IsNullOrEmpty(_secondSubtitleFileName))
            {
                return;
            }

            DoMpvCommand("sub-remove");
        }

        public void ReloadSubtitle()
        {
            DoMpvCommand("sub-reload");
        }

        public void HideCursor()
        {
            _mpvSetOptionString(_mpvHandle, GetUtf8Bytes("cursor-autohide"), GetUtf8Bytes("always"));
        }

        public void ShowCursor()
        {
            _mpvSetOptionString(_mpvHandle, GetUtf8Bytes("cursor-autohide"), GetUtf8Bytes("no"));
        }

        public static bool IsInstalled
        {
            get
            {
                try
                {
                    if (Configuration.IsRunningOnLinux)
                    {
                        return LoadLib();
                    }

                    var dllFile = GetMpvPath("mpv-1.dll");
                    return File.Exists(dllFile);
                }
                catch 
                {
                    return false;
                }
            }
        }

        public static string GetMpvPath(string fileName)
        {
            if (Configuration.IsRunningOnWindows)
            {
                var path = Path.Combine(Configuration.DataDirectory, fileName);
                if (File.Exists(path))
                {
                    return path;
                }
            }

            return null;
        }

        private static bool LoadLib()
        {
            if (_libMpvDll == IntPtr.Zero)
            {
                if (Configuration.IsRunningOnWindows)
                {
                    _libMpvDll = NativeMethods.CrossLoadLibrary(GetMpvPath("mpv-1.dll"));
                    if (_libMpvDll == IntPtr.Zero)
                    {
                        // to work with e.g. cyrillic characters!
                        Directory.SetCurrentDirectory(Configuration.DataDirectory);
                        _libMpvDll = NativeMethods.CrossLoadLibrary("mpv-1.dll");
                    }
                }
                else
                {
                    _libMpvDll = NativeMethods.CrossLoadLibrary("libmpv.so");
                    if (_libMpvDll == IntPtr.Zero)
                    {
                        _libMpvDll = NativeMethods.CrossLoadLibrary("libmpv.so.1");
                    }

                    int i = 107;
                    while (_libMpvDll == IntPtr.Zero && i < 120)
                    {
                        _libMpvDll = NativeMethods.CrossLoadLibrary($"libmpv.so.1.{i}.0");
                        i++;
                    }
                }
            }

            return _libMpvDll != IntPtr.Zero;
        }

        public void Initialize(Control ownerControl, string videoFileName, EventHandler onVideoLoaded, EventHandler onVideoEnded)
        {
            _secondSubtitleFileName = null;
            if (LoadLib())
            {
                LoadLibMpvDynamic();
                if (!IsAllMethodsLoaded())
                {
                    throw new Exception("MPV - not all needed methods found in dll file");
                }
                _mpvHandle = _mpvCreate.Invoke();
            }
            else if (!Directory.Exists(videoFileName))
            {
                return;
            }

            OnVideoLoaded = onVideoLoaded;
            OnVideoEnded = onVideoEnded;

            if (!string.IsNullOrEmpty(videoFileName))
            {
                _mpvInitialize.Invoke(_mpvHandle);
                SetVideoOwner(ownerControl);

                _mpvSetOptionString(_mpvHandle, GetUtf8Bytes("keep-open"), GetUtf8Bytes("always")); // don't auto close video
                _mpvSetOptionString(_mpvHandle, GetUtf8Bytes("no-sub"), GetUtf8Bytes(string.Empty)); // don't load subtitles (does not seem to work anymore)
                _mpvSetOptionString(_mpvHandle, GetUtf8Bytes("sid"), GetUtf8Bytes("no")); // don't load subtitles

                if (videoFileName.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                    videoFileName.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    _mpvSetOptionString(_mpvHandle, GetUtf8Bytes("ytdl"), GetUtf8Bytes("yes"));
                }

                DoMpvCommand("loadfile", videoFileName);

                System.Threading.Thread.Sleep(100);
                SetVideoOwner(ownerControl);

                _videoLoadedTimer = new Timer { Interval = 50 };
                _videoLoadedTimer.Tick += VideoLoadedTimer_Tick;
                _videoLoadedTimer.Start();

                SetVideoOwner(ownerControl);
            }
        }

        private void SetVideoOwner(Control ownerControl)
        {
            if (ownerControl != null)
            {
                int iterations = 25;
                int returnCode = -1;
                int mpvFormatInt64 = 4;
                if (ownerControl.IsDisposed)
                {
                    return;
                }
                var windowId = ownerControl.Handle.ToInt64();
                while (returnCode != 0 && iterations > 0)
                {
                    Application.DoEvents();

                    lock (_lockObj)
                    {
                        if (_mpvHandle == IntPtr.Zero)
                        {
                            return;
                        }
                        returnCode = _mpvSetOption(_mpvHandle, GetUtf8Bytes("wid"), mpvFormatInt64, ref windowId);
                        if (returnCode != 0)
                        {
                            iterations--;
                            System.Threading.Thread.Sleep(100);
                        }
                    }
                }
            }
            Pause();
        }

        private void VideoLoadedTimer_Tick(object sender, EventArgs e)
        {
            _videoLoadedTimer.Stop();
            const int mpvEventFileLoaded = 8;
            int l = 0;
            while (l < 10000)
            {
                Application.DoEvents();
                try
                {
                    lock (_lockObj)
                    {
                        if (_mpvHandle == IntPtr.Zero)
                        {
                            return;
                        }

                        var eventIdHandle = _mpvWaitEvent(_mpvHandle, 0);
                        var eventId = Convert.ToInt64(Marshal.PtrToStructure(eventIdHandle, typeof(int)));
                        if (eventId == mpvEventFileLoaded)
                        {
                            break;
                        }
                    }
                    l++;
                }
                catch
                {
                    return;
                }
            }
            Application.DoEvents();
            OnVideoLoaded?.Invoke(this, null);
            Application.DoEvents();
            Pause();
        }

        public void DisposeVideoPlayer()
        {
            Dispose();
        }

        private readonly object _lockObj = new object();
        private void ReleaseUnmanagedResources()
        {
            try
            {
                lock (_lockObj)
                {
                    if (_mpvHandle != IntPtr.Zero)
                    {
                        HardDispose();
                    }
                }
            }
            catch
            {
                // ignored
            }
        }

        ~LibMpvDynamic()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public void HardDispose()
        {
            DoMpvCommand("stop");
            System.Threading.Thread.Sleep(150);
            Application.DoEvents();
            DoMpvCommand("quit");
            _mpvHandle = IntPtr.Zero;
            System.Threading.Thread.Sleep(150);
            Application.DoEvents();
            System.Threading.Thread.Sleep(50);
            Application.DoEvents();
        }

        protected virtual void Dispose(bool disposing)
        {
            ReleaseUnmanagedResources();
        }

        public int ToggleBrightness()
        {
            _brightness += 25;
            if (_brightness > 100)
            {
                _brightness = -100;
            }

            _mpvSetOptionString(_mpvHandle, GetUtf8Bytes("brightness"), GetUtf8Bytes(_brightness.ToString(CultureInfo.InvariantCulture)));
            return _brightness;
        }

        public int ToggleContrast()
        {
            _contrast += 25;
            if (_contrast > 100)
            {
                _contrast = -100;
            }

            _mpvSetOptionString(_mpvHandle, GetUtf8Bytes("contrast"), GetUtf8Bytes(_contrast.ToString(CultureInfo.InvariantCulture)));
            return _contrast;
        }
    }
}
