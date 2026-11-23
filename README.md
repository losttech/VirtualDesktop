## VirtualDesktop

VirtualDesktop wraps the Windows shell's virtual desktop API for desktop apps. [Microsoft's IVirtualDesktopManager reference](https://learn.microsoft.com/en-us/windows/win32/api/shobjidl_core/nn-shobjidl_core-ivirtualdesktopmanager)

## Features

* Look up the desktop for a top-level window and check whether it is on the current desktop.
* Move a window owned by the calling process to a known desktop.
* Observe changes to the current desktop.

## Installation

Install NuGet package(s).

```powershell
PM> Install-Package VirtualDesktop
```

* [VirtualDesktop](https://www.nuget.org/packages/VirtualDesktop/) - Core classes for VirtualDesktop.
* [VirtualDesktop.WPF](https://www.nuget.org/packages/VirtualDesktop.WPF/) - Provides extension methods for WPF [Window class](https://msdn.microsoft.com/en-us/library/system.windows.window(v=vs.110).aspx).
* [VirtualDesktop.WinForms](https://www.nuget.org/packages/VirtualDesktop.WinForms/) - Provides extension methods for [Form class](https://msdn.microsoft.com/en-us/library/system.windows.forms.form(v=vs.110).aspx).



## How to use

Preparation: 
```xml
<!-- Please create application manifest file and run without debugging. -->
<compatibility xmlns="urn:schemas-microsoft-com:compatibility.v1">
    <application>
	    <!-- Windows 10 -->
	    <supportedOS Id="{8e0f7a12-bfb3-4fe8-b9a5-48fd50a15a9a}" />
    </application>
</compatibility>
```
```csharp
using WindowsDesktop;
```

Query a window's desktop

```csharp
var desktop = VirtualDesktop.FromHwnd(hwnd);
var isCurrent = VirtualDesktop.IsWindowOnCurrentVirtualDesktop(hwnd);
```

Callers migrating from `TryGetIdFromHwnd` can use `IdFromHwnd` to get a nullable GUID. During an Explorer restart, both queries can return `null`. The Boolean `IsCurrentVirtualDesktop` helper returns `false` in that case. If Windows has no registered virtual desktop manager, `IsPresent` returns `false` and queries for a window throw `NotSupportedException`. Other shell failures propagate as managed exceptions.

`CurrentID` returns `Guid.Empty` until the library identifies a desktop. Subscribe to `CurrentChanged` and then read `CurrentID` to cover both startup and later changes. The first observation has a null `OldDesktop`. UI subscribers receive events on their subscribing synchronization context. Subscribers without a context receive them on the listener thread.

for WPF window
```csharp
// Need to install 'VirtualDesktop.WPF' package

// Check whether a window is on the current desktop.
var isCurrent = window.IsCurrentVirtualDesktop();

// Get Virtual Desktop for WPF window
var desktop = window.GetCurrentDesktop();

// Move window to specific Virtual Desktop
window.MoveToDesktop(desktop);
```

See also:
* [samples/VirtualDesktop.Showcase](samples/VirtualDesktop.Showcase) project  
![ss150904031950kd](https://cloud.githubusercontent.com/assets/1779073/9666915/d57850d8-52b3-11e5-9d61-b13a49656b11.png)


## License

This library is under [the MIT License](https://github.com/Grabacr07/VirtualDesktop/blob/master/LICENSE).
