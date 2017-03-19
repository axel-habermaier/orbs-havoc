// The MIT License (MIT)
// 
// Copyright (c) 2012-2017, Axel Habermaier
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

namespace OrbsHavoc.Platform
{
	// ReSharper disable InconsistentNaming
	// ReSharper disable MemberCanBePrivate.Global
	// ReSharper disable FieldCanBeMadeReadOnly.Global
	using System;
	using System.Diagnostics;
	using System.Runtime.InteropServices;
	using Input;
	using JetBrains.Annotations;
	using Logging;
	using Memory;

	/// <summary>
	///   Provides access to SDL2 functions, constants, and types.
	/// </summary>
	/// <remarks>
	///   Based on SDL2# - C# Wrapper for SDL2
	///   https://github.com/flibitijibibo/SDL2-CS
	///   zlib license
	/// </remarks>
	[UsedImplicitly(ImplicitUseTargetFlags.Members)]
	public static unsafe class SDL2
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate int SDL_EventFilter(void* userdata, void* sdlevent);

		public delegate uint SDL_TimerCallback(uint interval, void* param);

		private const string LibraryName = "SDL2.dll";
		public const int SDL_NUM_SCANCODES = 512;
		public const uint SDL_INIT_TIMER = 0x00000001;
		public const uint SDL_INIT_AUDIO = 0x00000010;
		public const uint SDL_INIT_VIDEO = 0x00000020;
		public const uint SDL_INIT_JOYSTICK = 0x00000200;
		public const uint SDL_INIT_HAPTIC = 0x00001000;
		public const uint SDL_INIT_GAMECONTROLLER = 0x00002000;
		public const uint SDL_INIT_NOPARACHUTE = 0x00100000;
		public const string SDL_HINT_FRAMEBUFFER_ACCELERATION = "SDL_FRAMEBUFFER_ACCELERATION";
		public const string SDL_HINT_RENDER_DRIVER = "SDL_RENDER_DRIVER";
		public const string SDL_HINT_RENDER_OPENGL_SHADERS = "SDL_RENDER_OPENGL_SHADERS";
		public const string SDL_HINT_RENDER_DIRECT3D_THREADSAFE = "SDL_RENDER_DIRECT3D_THREADSAFE";
		public const string SDL_HINT_RENDER_VSYNC = "SDL_RENDER_VSYNC";
		public const string SDL_HINT_VIDEO_X11_XVIDMODE = "SDL_VIDEO_X11_XVIDMODE";
		public const string SDL_HINT_VIDEO_X11_XINERAMA = "SDL_VIDEO_X11_XINERAMA";
		public const string SDL_HINT_VIDEO_X11_XRANDR = "SDL_VIDEO_X11_XRANDR";
		public const string SDL_HINT_GRAB_KEYBOARD = "SDL_GRAB_KEYBOARD";
		public const string SDL_HINT_VIDEO_MINIMIZE_ON_FOCUS_LOSS = "SDL_VIDEO_MINIMIZE_ON_FOCUS_LOSS";
		public const string SDL_HINT_IDLE_TIMER_DISABLED = "SDL_IOS_IDLE_TIMER_DISABLED";
		public const string SDL_HINT_ORIENTATIONS = "SDL_IOS_ORIENTATIONS";
		public const string SDL_HINT_XINPUT_ENABLED = "SDL_XINPUT_ENABLED";
		public const string SDL_HINT_GAMECONTROLLERCONFIG = "SDL_GAMECONTROLLERCONFIG";
		public const string SDL_HINT_JOYSTICK_ALLOW_BACKGROUND_EVENTS = "SDL_JOYSTICK_ALLOW_BACKGROUND_EVENTS";
		public const string SDL_HINT_ALLOW_TOPMOST = "SDL_ALLOW_TOPMOST";
		public const string SDL_HINT_TIMER_RESOLUTION = "SDL_TIMER_RESOLUTION";
		public const string SDL_HINT_RENDER_SCALE_QUALITY = "SDL_RENDER_SCALE_QUALITY";
		public const string SDL_HINT_VIDEO_HIGHDPI_DISABLED = "SDL_VIDEO_HIGHDPI_DISABLED";
		public const string SDL_HINT_CTRL_CLICK_EMULATE_RIGHT_CLICK = "SDL_CTRL_CLICK_EMULATE_RIGHT_CLICK";
		public const string SDL_HINT_VIDEO_WIN_D3DCOMPILER = "SDL_VIDEO_WIN_D3DCOMPILER";
		public const string SDL_HINT_MOUSE_RELATIVE_MODE_WARP = "SDL_MOUSE_RELATIVE_MODE_WARP";
		public const string SDL_HINT_VIDEO_WINDOW_SHARE_PIXEL_FORMAT = "SDL_VIDEO_WINDOW_SHARE_PIXEL_FORMAT";
		public const string SDL_HINT_VIDEO_ALLOW_SCREENSAVER = "SDL_VIDEO_ALLOW_SCREENSAVER";
		public const string SDL_HINT_ACCELEROMETER_AS_JOYSTICK = "SDL_ACCELEROMETER_AS_JOYSTICK";
		public const string SDL_HINT_VIDEO_MAC_FULLSCREEN_SPACES = "SDL_VIDEO_MAC_FULLSCREEN_SPACES";
		public const int SDL_HINT_DEFAULT = 0;
		public const int SDL_HINT_NORMAL = 1;
		public const int SDL_HINT_OVERRIDE = 2;
		public const int SDL_MESSAGEBOX_ERROR = 0x00000010;
		public const int SDL_MESSAGEBOX_WARNING = 0x00000020;
		public const int SDL_MESSAGEBOX_INFORMATION = 0x00000040;
		public const int SDL_MAJOR_VERSION = 2;
		public const int SDL_MINOR_VERSION = 0;
		public const int SDL_PATCHLEVEL = 3;
		public const int SDL_GL_RED_SIZE = 0;
		public const int SDL_GL_GREEN_SIZE = 1;
		public const int SDL_GL_BLUE_SIZE = 2;
		public const int SDL_GL_ALPHA_SIZE = 3;
		public const int SDL_GL_BUFFER_SIZE = 4;
		public const int SDL_GL_DOUBLEBUFFER = 5;
		public const int SDL_GL_DEPTH_SIZE = 6;
		public const int SDL_GL_STENCIL_SIZE = 7;
		public const int SDL_GL_ACCUM_RED_SIZE = 8;
		public const int SDL_GL_ACCUM_GREEN_SIZE = 9;
		public const int SDL_GL_ACCUM_BLUE_SIZE = 10;
		public const int SDL_GL_ACCUM_ALPHA_SIZE = 11;
		public const int SDL_GL_STEREO = 12;
		public const int SDL_GL_MULTISAMPLEBUFFERS = 13;
		public const int SDL_GL_MULTISAMPLESAMPLES = 14;
		public const int SDL_GL_ACCELERATED_VISUAL = 15;
		public const int SDL_GL_RETAINED_BACKING = 16;
		public const int SDL_GL_CONTEXT_MAJOR_VERSION = 17;
		public const int SDL_GL_CONTEXT_MINOR_VERSION = 18;
		public const int SDL_GL_CONTEXT_EGL = 19;
		public const int SDL_GL_CONTEXT_FLAGS = 20;
		public const int SDL_GL_CONTEXT_PROFILE_MASK = 21;
		public const int SDL_GL_SHARE_WITH_CURRENT_CONTEXT = 22;
		public const int SDL_GL_FRAMEBUFFER_SRGB_CAPABLE = 23;
		public const int SDL_GL_CONTEXT_PROFILE_CORE = 0x0001;
		public const int SDL_GL_CONTEXT_PROFILE_COMPATIBILITY = 0x0002;
		public const int SDL_GL_CONTEXT_PROFILE_ES = 0x0004;
		public const int SDL_GL_CONTEXT_DEBUG_FLAG = 0x0001;
		public const int SDL_GL_CONTEXT_FORWARD_COMPATIBLE_FLAG = 0x0002;
		public const int SDL_GL_CONTEXT_ROBUST_ACCESS_FLAG = 0x0004;
		public const int SDL_GL_CONTEXT_RESET_ISOLATION_FLAG = 0x0008;
		public const int SDL_WINDOWEVENT_NONE = 0;
		public const int SDL_WINDOWEVENT_SHOWN = 1;
		public const int SDL_WINDOWEVENT_HIDDEN = 2;
		public const int SDL_WINDOWEVENT_EXPOSED = 3;
		public const int SDL_WINDOWEVENT_MOVED = 4;
		public const int SDL_WINDOWEVENT_RESIZED = 5;
		public const int SDL_WINDOWEVENT_SIZE_CHANGED = 6;
		public const int SDL_WINDOWEVENT_MINIMIZED = 7;
		public const int SDL_WINDOWEVENT_MAXIMIZED = 8;
		public const int SDL_WINDOWEVENT_RESTORED = 9;
		public const int SDL_WINDOWEVENT_ENTER = 10;
		public const int SDL_WINDOWEVENT_LEAVE = 11;
		public const int SDL_WINDOWEVENT_FOCUS_GAINED = 12;
		public const int SDL_WINDOWEVENT_FOCUS_LOST = 13;
		public const int SDL_WINDOWEVENT_CLOSE = 14;
		public const uint SDL_WINDOW_FULLSCREEN = 0x00000001;
		public const uint SDL_WINDOW_OPENGL = 0x00000002;
		public const uint SDL_WINDOW_SHOWN = 0x00000004;
		public const uint SDL_WINDOW_HIDDEN = 0x00000008;
		public const uint SDL_WINDOW_BORDERLESS = 0x00000010;
		public const uint SDL_WINDOW_RESIZABLE = 0x00000020;
		public const uint SDL_WINDOW_MINIMIZED = 0x00000040;
		public const uint SDL_WINDOW_MAXIMIZED = 0x00000080;
		public const uint SDL_WINDOW_INPUT_GRABBED = 0x00000100;
		public const uint SDL_WINDOW_INPUT_FOCUS = 0x00000200;
		public const uint SDL_WINDOW_MOUSE_FOCUS = 0x00000400;
		public const uint SDL_WINDOW_FULLSCREEN_DESKTOP = (SDL_WINDOW_FULLSCREEN | 0x00001000);
		public const uint SDL_WINDOW_FOREIGN = 0x00000800;
		public const uint SDL_WINDOW_ALLOW_HIGHDPI = 0x00002000;
		public const int SDL_WINDOWPOS_UNDEFINED_MASK = 0x1FFF0000;
		public const int SDL_WINDOWPOS_CENTERED_MASK = 0x2FFF0000;
		public const int SDL_WINDOWPOS_UNDEFINED = 0x1FFF0000;
		public const int SDL_WINDOWPOS_CENTERED = 0x2FFF0000;
		public const uint SDL_SWSURFACE = 0x00000000;
		public const uint SDL_PREALLOC = 0x00000001;
		public const uint SDL_RLEACCEL = 0x00000002;
		public const uint SDL_DONTFREE = 0x00000004;
		public const byte SDL_PRESSED = 1;
		public const byte SDL_RELEASED = 0;
		public const int SDL_TEXTEDITINGEVENT_TEXT_SIZE = 32;
		public const int SDL_TEXTINPUTEVENT_TEXT_SIZE = 32;
		public const uint SDL_FIRSTEVENT = 0;
		public const uint SDL_QUIT = 0x100;
		public const uint SDL_WINDOWEVENT = 0x200;
		public const uint SDL_SYSWMEVENT = 0x201;
		public const uint SDL_KEYDOWN = 0x300;
		public const uint SDL_KEYUP = 0x301;
		public const uint SDL_TEXTEDITING = 0x302;
		public const uint SDL_TEXTINPUT = 0x303;
		public const uint SDL_MOUSEMOTION = 0x400;
		public const uint SDL_MOUSEBUTTONDOWN = 0x401;
		public const uint SDL_MOUSEBUTTONUP = 0x402;
		public const uint SDL_MOUSEWHEEL = 0x403;
		public const uint SDL_JOYAXISMOTION = 0x600;
		public const uint SDL_JOYBALLMOTION = 0x601;
		public const uint SDL_JOYHATMOTION = 0x602;
		public const uint SDL_JOYBUTTONDOWN = 0x603;
		public const uint SDL_JOYBUTTONUP = 0x604;
		public const uint SDL_JOYDEVICEADDED = 0x605;
		public const uint SDL_JOYDEVICEREMOVED = 0x606;
		public const uint SDL_CONTROLLERAXISMOTION = 0x650;
		public const uint SDL_CONTROLLERBUTTONDOWN = 0x651;
		public const uint SDL_CONTROLLERBUTTONUP = 0x652;
		public const uint SDL_CONTROLLERDEVICEADDED = 0x653;
		public const uint SDL_CONTROLLERDEVICEREMOVED = 0x654;
		public const uint SDL_CONTROLLERDEVICEREMAPPED = 0x655;
		public const uint SDL_FINGERDOWN = 0x700;
		public const uint SDL_FINGERUP = 0x701;
		public const uint SDL_FINGERMOTION = 0x702;
		public const uint SDL_DOLLARGESTURE = 0x800;
		public const uint SDL_DOLLARRECORD = 0x801;
		public const uint SDL_MULTIGESTURE = 0x802;
		public const uint SDL_CLIPBOARDUPDATE = 0x900;
		public const uint SDL_DROPFILE = 0x1000;
		public const uint SDL_RENDER_TARGETS_RESET = 0x2000;
		public const uint SDL_USEREVENT = 0x8000;
		public const uint SDL_LASTEVENT = 0xFFFF;
		public const int SDL_ADDEVENT = 0;
		public const int SDL_PEEKEVENT = 1;
		public const int SDL_GETEVENT = 2;
		public const int SDL_QUERY = -1;
		public const int SDL_IGNORE = 0;
		public const int SDL_DISABLE = 0;
		public const int SDL_ENABLE = 1;
		public const int SDLK_SCANCODE_MASK = (1 << 30);
		public const uint SDL_BUTTON_LEFT = 1;
		public const uint SDL_BUTTON_MIDDLE = 2;
		public const uint SDL_BUTTON_RIGHT = 3;
		public const uint SDL_BUTTON_X1 = 4;
		public const uint SDL_BUTTON_X2 = 5;
		public const uint SDL_TOUCH_MOUSEID = uint.MaxValue;
		public const byte SDL_HAT_CENTERED = 0x00;
		public const byte SDL_HAT_UP = 0x01;
		public const byte SDL_HAT_RIGHT = 0x02;
		public const byte SDL_HAT_DOWN = 0x04;
		public const byte SDL_HAT_LEFT = 0x08;
		public const byte SDL_HAT_RIGHTUP = SDL_HAT_RIGHT | SDL_HAT_UP;
		public const byte SDL_HAT_RIGHTDOWN = SDL_HAT_RIGHT | SDL_HAT_DOWN;
		public const byte SDL_HAT_LEFTUP = SDL_HAT_LEFT | SDL_HAT_UP;
		public const byte SDL_HAT_LEFTDOWN = SDL_HAT_LEFT | SDL_HAT_DOWN;
		public const int SDL_CONTROLLER_BINDTYPE_NONE = 0;
		public const int SDL_CONTROLLER_BINDTYPE_BUTTON = 1;
		public const int SDL_CONTROLLER_BINDTYPE_AXIS = 2;
		public const int SDL_CONTROLLER_BINDTYPE_HAT = 3;
		public const int SDL_CONTROLLER_AXIS_INVALID = -1;
		public const int SDL_CONTROLLER_AXIS_LEFTX = 0;
		public const int SDL_CONTROLLER_AXIS_LEFTY = 1;
		public const int SDL_CONTROLLER_AXIS_RIGHTX = 2;
		public const int SDL_CONTROLLER_AXIS_RIGHTY = 3;
		public const int SDL_CONTROLLER_AXIS_TRIGGERLEFT = 4;
		public const int SDL_CONTROLLER_AXIS_TRIGGERRIGHT = 5;
		public const int SDL_CONTROLLER_AXIS_MAX = 6;
		public const int SDL_CONTROLLER_BUTTON_INVALID = -1;
		public const int SDL_CONTROLLER_BUTTON_A = 0;
		public const int SDL_CONTROLLER_BUTTON_B = 1;
		public const int SDL_CONTROLLER_BUTTON_X = 2;
		public const int SDL_CONTROLLER_BUTTON_Y = 3;
		public const int SDL_CONTROLLER_BUTTON_BACK = 4;
		public const int SDL_CONTROLLER_BUTTON_GUIDE = 5;
		public const int SDL_CONTROLLER_BUTTON_START = 6;
		public const int SDL_CONTROLLER_BUTTON_LEFTSTICK = 7;
		public const int SDL_CONTROLLER_BUTTON_RIGHTSTICK = 8;
		public const int SDL_CONTROLLER_BUTTON_LEFTSHOULDER = 9;
		public const int SDL_CONTROLLER_BUTTON_RIGHTSHOULDER = 10;
		public const int SDL_CONTROLLER_BUTTON_DPAD_UP = 11;
		public const int SDL_CONTROLLER_BUTTON_DPAD_DOWN = 12;
		public const int SDL_CONTROLLER_BUTTON_DPAD_LEFT = 13;
		public const int SDL_CONTROLLER_BUTTON_DPAD_RIGHT = 14;
		public const int SDL_CONTROLLER_BUTTON_MAX = 15;
		public const ushort SDL_HAPTIC_CONSTANT = (1 << 0);
		public const ushort SDL_HAPTIC_SINE = (1 << 1);
		public const ushort SDL_HAPTIC_LEFTRIGHT = (1 << 2);
		public const ushort SDL_HAPTIC_TRIANGLE = (1 << 3);
		public const ushort SDL_HAPTIC_SAWTOOTHUP = (1 << 4);
		public const ushort SDL_HAPTIC_SAWTOOTHDOWN = (1 << 5);
		public const ushort SDL_HAPTIC_SPRING = (1 << 7);
		public const ushort SDL_HAPTIC_DAMPER = (1 << 8);
		public const ushort SDL_HAPTIC_INERTIA = (1 << 9);
		public const ushort SDL_HAPTIC_FRICTION = (1 << 10);
		public const ushort SDL_HAPTIC_CUSTOM = (1 << 11);
		public const ushort SDL_HAPTIC_GAIN = (1 << 12);
		public const ushort SDL_HAPTIC_AUTOCENTER = (1 << 13);
		public const ushort SDL_HAPTIC_STATUS = (1 << 14);
		public const ushort SDL_HAPTIC_PAUSE = (1 << 15);
		public const byte SDL_HAPTIC_POLAR = 0;
		public const byte SDL_HAPTIC_CARTESIAN = 1;
		public const byte SDL_HAPTIC_SPHERICAL = 2;
		public const uint SDL_HAPTIC_INFINITY = 4292967295U;
		public static readonly int SDL_COMPILEDVERSION = SDL_VERSIONNUM(SDL_MAJOR_VERSION, SDL_MINOR_VERSION, SDL_PATCHLEVEL);
		public static readonly uint SDL_BUTTON_LMASK = SDL_BUTTON(SDL_BUTTON_LEFT);
		public static readonly uint SDL_BUTTON_MMASK = SDL_BUTTON(SDL_BUTTON_MIDDLE);
		public static readonly uint SDL_BUTTON_RMASK = SDL_BUTTON(SDL_BUTTON_RIGHT);
		public static readonly uint SDL_BUTTON_X1MASK = SDL_BUTTON(SDL_BUTTON_X1);
		public static readonly uint SDL_BUTTON_X2MASK = SDL_BUTTON(SDL_BUTTON_X2);

		public static IDisposable Initialize()
		{
			try
			{
				if (SDL_Init(SDL_INIT_VIDEO) != 0)
					Log.Die($"SDL2 initialization failed: {SDL_GetError()}");

				SDL_GetVersion(out var version);

				const int major = 2, minor = 0, patch = 2;
				if (!SDL_VERSION_ATLEAST(major, minor, patch))
				{
					Log.Die($"SDL2 is outdated: Version {version.major}.{version.minor}.{version.patch} is installed but at " +
							$"least version {major}.{minor}.{patch} is required.");
				}

				SDL_GL_SetAttribute(SDL_GL_RED_SIZE, 8);
				SDL_GL_SetAttribute(SDL_GL_GREEN_SIZE, 8);
				SDL_GL_SetAttribute(SDL_GL_BLUE_SIZE, 8);
				SDL_GL_SetAttribute(SDL_GL_DEPTH_SIZE, 0);
				SDL_GL_SetAttribute(SDL_GL_STENCIL_SIZE, 0);
				SDL_GL_SetAttribute(SDL_GL_CONTEXT_MAJOR_VERSION, 3);
				SDL_GL_SetAttribute(SDL_GL_CONTEXT_MINOR_VERSION, 3);
				SDL_GL_SetAttribute(SDL_GL_CONTEXT_PROFILE_MASK, SDL_GL_CONTEXT_PROFILE_CORE);

#if DEBUG
				SDL_GL_SetAttribute(SDL_GL_CONTEXT_FLAGS, SDL_GL_CONTEXT_FORWARD_COMPATIBLE_FLAG | SDL_GL_CONTEXT_DEBUG_FLAG);
#else
				SDL_GL_SetAttribute(SDL_GL_CONTEXT_FLAGS, SDL_GL_CONTEXT_FORWARD_COMPATIBLE_FLAG);
#endif

				SDL_StopTextInput();
				if (SDL_GL_LoadLibrary(null) != 0)
					Log.Die($"Failed to load the OpenGL library: {SDL_GetError()}");

				Log.Info($"SDL {version.major}.{version.minor}.{version.patch} initialized.");
				return new PlatformCleanup();
			}
			catch (DllNotFoundException)
			{
				Log.Die("The SDL2 library could not be not found. Make sure SDL2 is installed on your " +
						"system or 'SDL2.dll' is placed in the application directory. On Windows, make sure " +
						"that you have the latest Microsoft Visual C++ Redistributable (x64) installed.");
			}

			return null;
		}

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_Init(uint flags);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_Quit();

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern uint SDL_WasInit(uint flags);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_ClearHints();

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern byte* SDL_GetHint(byte* name);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_SetHint(byte* name, byte* value);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_SetHintWithPriority(byte* name, byte* value, int priority);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_ClearError();

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GetError")]
		private static extern byte* GetError();

		[DebuggerHidden]
		public static string SDL_GetError()
		{
			var message = Interop.ToString(GetError()).Trim().Replace("\r", "");

			if (message.EndsWith(".."))
				return message.Substring(0, message.Length - 1);

			if (!message.EndsWith(".") && !message.EndsWith("!") && !message.EndsWith("?"))
				return message + ".";

			return message;
		}

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_ShowSimpleMessageBox(int flags, byte* title, byte* message, void* window);

		public static void SDL_VERSION(out SDL_version x)
		{
			x.major = SDL_MAJOR_VERSION;
			x.minor = SDL_MINOR_VERSION;
			x.patch = SDL_PATCHLEVEL;
		}

		public static int SDL_VERSIONNUM(int X, int Y, int Z)
		{
			return (X * 1000) + (Y * 100) + Z;
		}

		public static bool SDL_VERSION_ATLEAST(int X, int Y, int Z)
		{
			return (SDL_COMPILEDVERSION >= SDL_VERSIONNUM(X, Y, Z));
		}

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_GetVersion(out SDL_version ver);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern byte* SDL_GetRevision();

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GetRevisionNumber();

		public static int SDL_WINDOWPOS_UNDEFINED_DISPLAY(int X)
		{
			return (SDL_WINDOWPOS_UNDEFINED_MASK | X);
		}

		public static bool SDL_WINDOWPOS_ISUNDEFINED(int X)
		{
			return (X & 0xFFFF0000) == SDL_WINDOWPOS_UNDEFINED_MASK;
		}

		public static int SDL_WINDOWPOS_CENTERED_DISPLAY(int X)
		{
			return (SDL_WINDOWPOS_CENTERED_MASK | X);
		}

		public static bool SDL_WINDOWPOS_ISCENTERED(int X)
		{
			return (X & 0xFFFF0000) == SDL_WINDOWPOS_CENTERED_MASK;
		}

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void* SDL_CreateWindow(byte* title, int x, int y, int w, int h, uint flags);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_DestroyWindow(void* window);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void* SDL_GetClosestDisplayMode(int displayIndex, ref SDL_DisplayMode mode, out SDL_DisplayMode closest);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GetCurrentDisplayMode(int displayIndex, out SDL_DisplayMode mode);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GetDesktopDisplayMode(int displayIndex, out SDL_DisplayMode mode);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GetDisplayBounds(int displayIndex, out SDL_Rect rect);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GetDisplayMode(int displayIndex, int modeIndex, out SDL_DisplayMode mode);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GetNumDisplayModes(int displayIndex);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GetNumVideoDisplays();

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GetNumVideoDrivers();

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern float SDL_GetWindowBrightness(void* window);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void* SDL_GetWindowData(void* window, byte* name);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GetWindowDisplayIndex(void* window);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GetWindowDisplayMode(void* window, out SDL_DisplayMode mode);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GL_LoadLibrary(void* path);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_GL_UnloadLibrary();

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern uint SDL_GetWindowFlags(void* window);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void* SDL_GetWindowFromID(uint id);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GetWindowGammaRamp(
			void* window,
			[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U2, SizeConst = 256)] ushort[] red,
			[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U2, SizeConst = 256)] ushort[] green,
			[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U2, SizeConst = 256)] ushort[] blue);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GetWindowGrab(void* window);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern uint SDL_GetWindowID(void* window);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern uint SDL_GetWindowPixelFormat(void* window);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_GetWindowMaximumSize(void* window, out int max_w, out int max_h);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_GetWindowMinimumSize(void* window, out int min_w, out int min_h);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_GetWindowPosition(void* window, out int x, out int y);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_GetWindowSize(void* window, out int w, out int h);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void* SDL_GetWindowSurface(void* window);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern byte* SDL_GetWindowTitle(void* window);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GL_BindTexture(void* texture, out float texw, out float texh);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void* SDL_GL_CreateContext(void* window);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_GL_DeleteContext(void* context);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void* SDL_GL_GetProcAddress(byte* proc);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GL_ExtensionSupported(byte* extension);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_GL_ResetAttributes();

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GL_GetAttribute(uint attr, out int value);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GL_GetSwapInterval();

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GL_MakeCurrent(void* window, void* context);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void* SDL_GL_GetCurrentWindow();

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void* SDL_GL_GetCurrentContext();

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_GL_GetDrawableSize(void* window, out int w, out int h);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GL_SetAttribute(uint attr, int value);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GL_SetSwapInterval(int interval);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_GL_SwapWindow(void* window);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GL_UnbindTexture(void* texture);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_HideWindow(void* window);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_IsScreenSaverEnabled();

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_MaximizeWindow(void* window);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_MinimizeWindow(void* window);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_RaiseWindow(void* window);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_RestoreWindow(void* window);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_SetWindowBrightness(void* window, float brightness);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void* SDL_SetWindowData(void* window, byte* name, void* userdata);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_SetWindowDisplayMode(void* window, ref SDL_DisplayMode mode);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_SetWindowFullscreen(void* window, uint flags);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_SetWindowGammaRamp(
			void* window,
			[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U2, SizeConst = 256)] ushort[] red,
			[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U2, SizeConst = 256)] ushort[] green,
			[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U2, SizeConst = 256)] ushort[] blue);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_SetWindowGrab(void* window, int grabbed);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_SetWindowIcon(void* window, void* icon);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_SetWindowMaximumSize(void* window, int max_w, int max_h);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_SetWindowMinimumSize(void* window, int min_w, int min_h);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_SetWindowPosition(void* window, int x, int y);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_SetWindowSize(void* window, int w, int h);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_SetWindowBordered(void* window, int bordered);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_SetWindowTitle(void* window, byte* title);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_ShowWindow(void* window);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_UpdateWindowSurface(void* window);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_UpdateWindowSurfaceRects(
			void* window,
			[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Struct, SizeParamIndex = 2)] SDL_Rect[] rects,
			int numrects);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_VideoInit(byte* driver_name);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_VideoQuit();

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void* SDL_CreateRGBSurfaceFrom(
			void* pixels,
			int width,
			int height,
			int depth,
			int pitch,
			uint Rmask,
			uint Gmask,
			uint Bmask,
			uint Amask
			);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_FreeSurface(void* surface);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_HasClipboardText();

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern byte* SDL_GetClipboardText();

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_SetClipboardText(byte* text);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_PumpEvents();

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_PeepEvents(
			[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Struct, SizeParamIndex = 1)] SDL_Event[] events,
			int numevents, int action, uint minType, uint maxType);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_HasEvent(uint type);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_HasEvents(uint minType, uint maxType);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_FlushEvent(uint type);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_FlushEvents(uint min, uint max);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_PollEvent(out SDL_Event _event);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_WaitEvent(out SDL_Event _event);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_WaitEventTimeout(out SDL_Event _event, int timeout);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_PushEvent(ref SDL_Event _event);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_SetEventFilter(SDL_EventFilter filter, void* userdata);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GetEventFilter(out SDL_EventFilter filter, out void* userdata);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_AddEventWatch(SDL_EventFilter filter, void* userdata);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_DelEventWatch(SDL_EventFilter filter, void* userdata);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_FilterEvents(SDL_EventFilter filter, void* userdata);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern byte SDL_EventState(uint type, int state);

		public static byte SDL_GetEventState(uint type)
		{
			return SDL_EventState(type, SDL_QUERY);
		}

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern uint SDL_RegisterEvents(int numevents);

		public static Key ScanCode_TO_KEYCODE(ScanCode X)
		{
			return (Key)((int)X | SDLK_SCANCODE_MASK);
		}

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void* SDL_GetKeyboardFocus();

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void* SDL_GetKeyboardState(out int numkeys);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern KeyModifiers SDL_GetModState();

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_SetModState(KeyModifiers modstate);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern Key SDL_GetKeyFromScancode(ScanCode scancode);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern ScanCode SDL_GetScancodeFromKey(Key key);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern byte* SDL_GetScancodeName(ScanCode scancode);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern ScanCode SDL_GetScancodeFromName(byte* name);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern byte* SDL_GetKeyName(Key key);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern Key SDL_GetKeyFromName(byte* name);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_StartTextInput();

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_IsTextInputActive();

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_StopTextInput();

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_SetTextInputRect(ref SDL_Rect rect);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_HasScreenKeyboardSupport();

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_IsScreenKeyboardShown(void* window);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void* SDL_GetMouseFocus();

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern uint SDL_GetMouseState(out int x, out int y);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern uint SDL_GetRelativeMouseState(out int x, out int y);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_WarpMouseInWindow(void* window, int x, int y);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_SetRelativeMouseMode(int enabled);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GetRelativeMouseMode();

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void* SDL_CreateCursor(void* data, void* mask, int w, int h, int hot_x, int hot_y);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void* SDL_CreateColorCursor(void* surface, int hot_x, int hot_y);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_SetCursor(void* cursor);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void* SDL_GetCursor();

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_FreeCursor(void* cursor);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_ShowCursor(int toggle);

		public static uint SDL_BUTTON(uint X)
		{
			return (uint)(1 << ((int)X - 1));
		}

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GetNumTouchDevices();

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern long SDL_GetTouchDevice(int index);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GetNumTouchFingers(long touchID);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void* SDL_GetTouchFinger(long touchID, int index);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_JoystickClose(void* joystick);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_JoystickEventState(int state);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern short SDL_JoystickGetAxis(void* joystick, int axis);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_JoystickGetBall(void* joystick, int ball, out int dx, out int dy);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern byte SDL_JoystickGetButton(void* joystick, int button);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern byte SDL_JoystickGetHat(void* joystick, int hat);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern byte* SDL_JoystickName(void* joystick);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern byte* SDL_JoystickNameForIndex(int device_index);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_JoystickNumAxes(void* joystick);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_JoystickNumBalls(void* joystick);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_JoystickNumButtons(void* joystick);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_JoystickNumHats(void* joystick);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void* SDL_JoystickOpen(int device_index);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_JoystickOpened(int device_index);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_JoystickUpdate();

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_NumJoysticks();

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern Guid SDL_JoystickGetDeviceGUID(int device_index);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern Guid SDL_JoystickGetGUID(void* joystick);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_JoystickGetGUIDString(Guid guid, byte[] pszGUID, int cbGUID);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern Guid SDL_JoystickGetGUIDFromString(byte* pchGUID);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_JoystickGetAttached(void* joystick);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_JoystickInstanceID(void* joystick);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GameControllerAddMapping(byte* mappingString);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern byte* SDL_GameControllerMappingForGUID(Guid guid);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern byte* SDL_GameControllerMapping(void* gamecontroller);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_IsGameController(int joystick_index);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern byte* SDL_GameControllerNameForIndex(int joystick_index);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void* SDL_GameControllerOpen(int joystick_index);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern byte* SDL_GameControllerName(void* gamecontroller);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GameControllerGetAttached(void* gamecontroller);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void* SDL_GameControllerGetJoystick(void* gamecontroller);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GameControllerEventState(int state);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_GameControllerUpdate();

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GameControllerGetAxisFromString(byte* pchString);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern byte* SDL_GameControllerGetStringForAxis(int axis);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern intBind SDL_GameControllerGetBindForAxis(void* gamecontroller, int axis);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern short SDL_GameControllerGetAxis(void* gamecontroller, int axis);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GameControllerGetButtonFromString(byte* pchString);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern byte* SDL_GameControllerGetStringForButton(int button);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern intBind SDL_GameControllerGetBindForButton(void* gamecontroller, int button);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern byte SDL_GameControllerGetButton(void* gamecontroller, int button);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_GameControllerClose(void* gamecontroller);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_HapticClose(void* haptic);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_HapticDestroyEffect(void* haptic, int effect);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_HapticEffectSupported(void* haptic, ref SDL_HapticEffect effect);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_HapticGetEffectStatus(void* haptic, int effect);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_HapticIndex(void* haptic);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern byte* SDL_HapticName(int device_index);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_HapticNewEffect(void* haptic, ref SDL_HapticEffect effect);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_HapticNumAxes(void* haptic);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_HapticNumEffects(void* haptic);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_HapticNumEffectsPlaying(void* haptic);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void* SDL_HapticOpen(int device_index);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_HapticOpened(int device_index);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void* SDL_HapticOpenFromJoystick(void* joystick);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void* SDL_HapticOpenFromMouse();

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_HapticPause(void* haptic);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern uint SDL_HapticQuery(void* haptic);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_HapticRumbleInit(void* haptic);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_HapticRumblePlay(void* haptic, float strength, uint length);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_HapticRumbleStop(void* haptic);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_HapticRumbleSupported(void* haptic);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_HapticRunEffect(void* haptic, int effect, uint iterations);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_HapticSetAutocenter(void* haptic, int autocenter);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_HapticSetGain(void* haptic, int gain);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_HapticStopAll(void* haptic);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_HapticStopEffect(void* haptic, int effect);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_HapticUnpause(void* haptic);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_HapticUpdateEffect(void* haptic, int effect, ref SDL_HapticEffect data);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_JoystickIsHaptic(void* joystick);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_MouseIsHaptic();

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_NumHaptics();

		public static bool SDL_TICKS_PASSED(uint A, uint B)
		{
			return ((int)(B - A) <= 0);
		}

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_Delay(uint ms);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern uint SDL_GetTicks();

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong SDL_GetPerformanceCounter();

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong SDL_GetPerformanceFrequency();

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_AddTimer(uint interval, SDL_TimerCallback callback, void* param);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_RemoveTimer(int id);

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GetCPUCount();

		[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int SDL_GetSystemRAM();

		private class PlatformCleanup : DisposableObject
		{
			protected override void OnDisposing()
			{
				try
				{
					SDL_GL_UnloadLibrary();
					SDL_Quit();
				}
				catch (DllNotFoundException)
				{
				}
			}
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_version
		{
			public byte major;
			public byte minor;
			public byte patch;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_DisplayMode
		{
			public uint format;
			public int w;
			public int h;
			public int refresh_rate;
			public void* driverdata;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_Point
		{
			public int x;
			public int y;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_Rect
		{
			public int x;
			public int y;
			public int w;
			public int h;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_Surface
		{
			public uint flags;
			public void* format;
			public int w;
			public int h;
			public int pitch;
			public void* pixels;
			public void* userdata;
			public int locked;
			public void* lock_data;
			public SDL_Rect clip_rect;
			public void* map;
			public int refcount;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_GenericEvent
		{
			public uint type;
			public uint timestamp;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_WindowEvent
		{
			public uint type;
			public uint timestamp;
			public uint windowID;
			public byte windowEvent;
			private readonly byte padding1;
			private readonly byte padding2;
			private readonly byte padding3;
			public int data1;
			public int data2;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_KeyboardEvent
		{
			public uint type;
			public uint timestamp;
			public uint windowID;
			public byte state;
			public byte repeat;
			private readonly byte padding2;
			private readonly byte padding3;
			public SDL_Keysym keysym;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_TextEditingEvent
		{
			public uint type;
			public uint timestamp;
			public uint windowID;
			public fixed byte text [SDL_TEXTEDITINGEVENT_TEXT_SIZE];
			public int start;
			public int length;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_TextInputEvent
		{
			public uint type;
			public uint timestamp;
			public uint windowID;
			public fixed byte text [SDL_TEXTINPUTEVENT_TEXT_SIZE];
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_MouseMotionEvent
		{
			public uint type;
			public uint timestamp;
			public uint windowID;
			public uint which;
			public byte state;
			private readonly byte padding1;
			private readonly byte padding2;
			private readonly byte padding3;
			public int x;
			public int y;
			public int xrel;
			public int yrel;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_MouseButtonEvent
		{
			public uint type;
			public uint timestamp;
			public uint windowID;
			public uint which;
			public MouseButton button;
			public byte state;
			public byte clicks;
			private readonly byte padding1;
			public int x;
			public int y;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_MouseWheelEvent
		{
			public uint type;
			public uint timestamp;
			public uint windowID;
			public uint which;
			public int x;
			public int y;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_JoyAxisEvent
		{
			public uint type;
			public uint timestamp;
			public int which;
			public byte axis;
			private readonly byte padding1;
			private readonly byte padding2;
			private readonly byte padding3;
			public short axisValue;
			public ushort padding4;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_JoyBallEvent
		{
			public uint type;
			public uint timestamp;
			public int which;
			public byte ball;
			private readonly byte padding1;
			private readonly byte padding2;
			private readonly byte padding3;
			public short xrel;
			public short yrel;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_JoyHatEvent
		{
			public uint type;
			public uint timestamp;
			public int which;
			public byte hat;
			public byte hatValue;
			private readonly byte padding1;
			private readonly byte padding2;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_JoyButtonEvent
		{
			public uint type;
			public uint timestamp;
			public int which;
			public byte button;
			public byte state;
			private readonly byte padding1;
			private readonly byte padding2;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_JoyDeviceEvent
		{
			public uint type;
			public uint timestamp;
			public int which;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_ControllerAxisEvent
		{
			public uint type;
			public uint timestamp;
			public int which;
			public byte axis;
			private readonly byte padding1;
			private readonly byte padding2;
			private readonly byte padding3;
			public short axisValue;
			private readonly ushort padding4;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_ControllerButtonEvent
		{
			public uint type;
			public uint timestamp;
			public int which;
			public byte button;
			public byte state;
			private readonly byte padding1;
			private readonly byte padding2;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_ControllerDeviceEvent
		{
			public uint type;
			public uint timestamp;

			public int which;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_TouchFingerEvent
		{
			public uint type;
			public uint timestamp;
			public long touchId;
			public long fingerId;
			public float x;
			public float y;
			public float dx;
			public float dy;
			public float pressure;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_MultiGestureEvent
		{
			public uint type;
			public uint timestamp;
			public long touchId;
			public float dTheta;
			public float dDist;
			public float x;
			public float y;
			public ushort numFingers;
			public ushort padding;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_DollarGestureEvent
		{
			public uint type;
			public uint timestamp;
			public long touchId;
			public long gestureId;
			public uint numFingers;
			public float error;
			public float x;
			public float y;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_DropEvent
		{
			public uint type;
			public uint timestamp;
			public void* file;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_QuitEvent
		{
			public uint type;
			public uint timestamp;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_UserEvent
		{
			public uint type;
			public uint timestamp;
			public uint windowID;
			public int code;
			public void* data1;
			public void* data2;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_SysWMEvent
		{
			public uint type;
			public uint timestamp;
			public void* msg;
		}

		[StructLayout(LayoutKind.Explicit)]
		public struct SDL_Event
		{
			[FieldOffset(0)]
			public uint type;

			[FieldOffset(0)]
			public SDL_WindowEvent window;

			[FieldOffset(0)]
			public SDL_KeyboardEvent key;

			[FieldOffset(0)]
			public SDL_TextEditingEvent edit;

			[FieldOffset(0)]
			public SDL_TextInputEvent text;

			[FieldOffset(0)]
			public SDL_MouseMotionEvent motion;

			[FieldOffset(0)]
			public SDL_MouseButtonEvent button;

			[FieldOffset(0)]
			public SDL_MouseWheelEvent wheel;

			[FieldOffset(0)]
			public SDL_JoyAxisEvent jaxis;

			[FieldOffset(0)]
			public SDL_JoyBallEvent jball;

			[FieldOffset(0)]
			public SDL_JoyHatEvent jhat;

			[FieldOffset(0)]
			public SDL_JoyButtonEvent jbutton;

			[FieldOffset(0)]
			public SDL_JoyDeviceEvent jdevice;

			[FieldOffset(0)]
			public SDL_ControllerAxisEvent caxis;

			[FieldOffset(0)]
			public SDL_ControllerButtonEvent cbutton;

			[FieldOffset(0)]
			public SDL_ControllerDeviceEvent cdevice;

			[FieldOffset(0)]
			public SDL_QuitEvent quit;

			[FieldOffset(0)]
			public SDL_UserEvent user;

			[FieldOffset(0)]
			public SDL_SysWMEvent syswm;

			[FieldOffset(0)]
			public SDL_TouchFingerEvent tfinger;

			[FieldOffset(0)]
			public SDL_MultiGestureEvent mgesture;

			[FieldOffset(0)]
			public SDL_DollarGestureEvent dgesture;

			[FieldOffset(0)]
			public SDL_DropEvent drop;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_Keysym
		{
			public ScanCode scancode;
			public Key sym;
			public KeyModifiers mod;
			public uint unicode;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_Finger
		{
			public long id;
			public float x;
			public float y;
			public float pressure;
		}

		[StructLayout(LayoutKind.Explicit)]
		public struct intBind
		{
			[FieldOffset(0)]
			public int bindType;

			[FieldOffset(4)]
			public int button;

			[FieldOffset(4)]
			public int axis;

			[FieldOffset(4)]
			public int hat;

			[FieldOffset(8)]
			public int hat_mask;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_HapticDirection
		{
			public byte type;
			public fixed int dir [3];
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_HapticConstant
		{
			public ushort type;
			public SDL_HapticDirection direction;
			public uint length;
			public ushort delay;
			public ushort button;
			public ushort interval;
			public short level;
			public ushort attack_length;
			public ushort attack_level;
			public ushort fade_length;
			public ushort fade_level;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_HapticPeriodic
		{
			public ushort type;
			public SDL_HapticDirection direction;
			public uint length;
			public ushort delay;
			public ushort button;
			public ushort interval;
			public ushort period;
			public short magnitude;
			public short offset;
			public ushort phase;
			public ushort attack_length;
			public ushort attack_level;
			public ushort fade_length;
			public ushort fade_level;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_HapticCondition
		{
			public ushort type;
			public SDL_HapticDirection direction;
			public uint length;
			public ushort delay;
			public ushort button;
			public ushort interval;
			public fixed ushort right_sat [3];
			public fixed ushort left_sat [3];
			public fixed short right_coeff [3];
			public fixed short left_coeff [3];
			public fixed ushort deadband [3];
			public fixed short center [3];
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_HapticRamp
		{
			public ushort type;
			public SDL_HapticDirection direction;
			public uint length;
			public ushort delay;
			public ushort button;
			public ushort interval;
			public short start;
			public short end;
			public ushort attack_length;
			public ushort attack_level;
			public ushort fade_length;
			public ushort fade_level;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_HapticLeftRight
		{
			public ushort type;
			public uint length;
			public ushort large_magnitude;
			public ushort small_magnitude;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_HapticCustom
		{
			public ushort type;
			public SDL_HapticDirection direction;
			public uint length;
			public ushort delay;
			public ushort button;
			public ushort interval;
			public byte channels;
			public ushort period;
			public ushort samples;
			public void* data;
			public ushort attack_length;
			public ushort attack_level;
			public ushort fade_length;
			public ushort fade_level;
		}

		[StructLayout(LayoutKind.Explicit)]
		public struct SDL_HapticEffect
		{
			[FieldOffset(0)]
			public ushort type;

			[FieldOffset(0)]
			public SDL_HapticConstant constant;

			[FieldOffset(0)]
			public SDL_HapticPeriodic periodic;

			[FieldOffset(0)]
			public SDL_HapticCondition condition;

			[FieldOffset(0)]
			public SDL_HapticRamp ramp;

			[FieldOffset(0)]
			public SDL_HapticLeftRight leftright;

			[FieldOffset(0)]
			public SDL_HapticCustom custom;
		}
	}
}