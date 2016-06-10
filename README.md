# XInput.Wrapper

- It is a short and monolithic C# class that can be embedded as source code in any project (1 source file).
- No external libraries, except XInput1_4.DLL but it ships today as a system component in Windows 8/8.1/10. It is available "inbox" and does not require redistribution with an application.
- To implement this class into your own project, just add the "X.cs" class file and start using it without any restriction. Anyway if you want you can add a reference to bin\XInput.Wrapper.dll.

# How to...

## Initialization

```c#
using XInput.Wrapper;

// for ease of later using
X.Gamepad gamepad = null;
```


Test of availability of XInput (xinput1_4.dll). Should not call often! This method not cached.

```c#
if (X.IsAvailable)
{
	...
```


Got Gamepad of the first user


```c#
if (X.IsAvailable)
{
	gamepad = X.Gamepad_1;
	...
```

Totaly available four devices: from X.Gamepad_1 to X.Gamepad_4.

Check gamepad's capabilites and test ForceFeedBack support

```c#
if (X.IsAvailable)
{
	gamepad = X.Gamepad_1;
	X.Gamepad.Capability caps = gamepad.Capabilities;

	if ((caps.Flags & X.Gamepad.CapabilityFlags.FFB_Supported) == X.Gamepad.CapabilityFlags.FFB_Supported)
	{
		// can play with ~~vibration~~ FFB
	}

	...
```


You can subscribe on events then start polling thread. X.StartPolling supports up to four devices.

```c#
if (X.IsAvailable)
{
	gamepad = X.Gamepad_1;

	...
	
    gamepad.KeyDown           += Gamepad_KeyDown;
	gamepad.StateChanged      += Gamepad_StateChanged;
	gamepad.ConnectionChanged += Gamepad_ConnectionChanged;

	X.StartPolling(gamepad);
}
```


Do not forget to stop polling

```c#
private void GameForm_FormClosing(object sender, FormClosingEventArgs e)
{
	if (gamepad != null)
		X.StopPolling();
		
	...
```


## Updating state of gamepad

If you are playing with event driven application (such as WinForms, WPF, etc) you can use X.Start|StopPolling. Application with custom main loop should use X.Gamepad.Update() method.

```c#
if (gamepad == null)
	return;

if (gamepad.Update())
{
	// something happened: button pressed, stick turned or trigger was triggered
}
```


If Update() returns TRUE you should check connection state, state of buttons, sticks, triggers, bumpers,...

```c#
if (gamepad.Update())
{
	// Check connection status and if connected then check battery state
	if (gamepad.IsConnected)
		gamepad.UpdateBattery();

	// KeyUp is an event that happens once
	// Try to stop vibrations
	if (gamepad.X_up)
		gamepad.FFB_Stop();

	// Proceed analogue inputs
	...
}
```