# XInput.Wrapper

- It is a short and monolithic C# class that can be embedded as source code in any project (1 source file).
- No external libraries, except XInput1_4.DLL but it ships today as a system component in Windows 8/8.1/10. It is available "inbox" and does not require redistribution with an application.
- To implement this class into your own project, just add the "X.cs" class file and start using it without any restriction. Anyway you can download [latest release](https://github.com/nikvoronin/xinput.wrapper/releases/latest) and add a reference to XInput.Wrapper.dll.

> An unstable `develop` branch is in front of the project by default. If you want to use wrapper in production see stable `master` branch or download it from [Releases](https://github.com/nikvoronin/XInput.Wrapper/releases) or use `nuget` packets.

# First steps

- [Getting Started With XInput](https://msdn.microsoft.com/ru-ru/library/windows/desktop/ee417001(v=vs.85).aspx)
- [Functions](https://msdn.microsoft.com/ru-ru/library/windows/desktop/ee417007(v=vs.85).aspx)

# How to...

## Initialization

```c#
using XInput.Wrapper;

// for ease of later using
X.Gamepad gamepad = null;
```


Test of availability of XInput 1.4 (xinput1_4.dll). Should not call often! This method not cached.

```c#
if (X.IsAvailable)
{
	...
```

> For performance reasons, don't call XInputGetState for an 'empty' user slot every frame. We recommend that you space out checks for new controllers every few seconds instead.<br/>
[MSDN](https://msdn.microsoft.com/en-us/library/windows/desktop/ee417001(v=vs.85).aspx#getting_controller_state)


Got Gamepad of the first user

```c#
if (X.IsAvailable)
{
	gamepad = X.Gamepad_1;
	...
```


Check gamepad's capabilites and test ForceFeedBack support

```c#
if (X.IsAvailable)
{
	gamepad = X.Gamepad_1;
	X.Gamepad.Capability caps = gamepad.Capabilities;

	if (gamepad.FFB_Supported)
	{
		// can play with ~~vibrations~~ FFB
	}

	...
```


You can subscribe on events then start polling thread. X.StartPolling() supports up to four controllers.

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


## Events

### ConnectionChanged

Occurs when state of connection is changed: controller connects or disconnects. Use X.GamePad.IsConnected() to retrieve state of connection.


### StateChanged

Occurs when controller sends new data packet for application. Something changed and you should do something with new data. You can spot that some of buttons were pressed (up or down), thumb was rotated or trigger was pressed. 


### KeyDown

When button pressed for a long time your app can regularly receive messages about this.

StateChanged event called once when button pressed. Use KeyDown event handler if you want to receive regularly messages about this.


## Update gamepad state 

If you are playing with an event driven application (such as WinForms, WPF, etc) you can use X.Start|StopPolling. Apps with custom main loop should call X.Gamepad.Update() method.

```c#
while (true)
{
	ProcessInput();
	Update();
	Render();
}

...

void ProcessInput()
{
	if (gamepad == null)
		return;

	if (gamepad.Update())
	{
		// something happened: button pressed, stick turned or trigger was triggered
	}
	...
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

	// You can process here but this will called once
	if (gamepad.A_down)
		gamepad.FFB_Vibrate(1, .5f, 100);

	// Processing of analog inputs
	...
}

// Will called again and again and again while button is pressed
if (gamepad.Buttons != 0)
{
	if (gamepad.X_down)
		gamepad.FFB_Vibrate(.2f, .5f, 100);
	
	...
}
```


## Processing of analog inputs

There are two kinds of analog methods: absolute and normalized. Normalized are with the _N postfix. Absolute methods returns X.Point {int X, int Y} objects, normalized returns X.PointF {float X, float Y}.

```
     0    <= LTrigger.X   <=   255		// absolute
     0.0f <= LTrigger_N.X <=     1.0f	// normalized

-32767    <= LStick.X     <= 32767		// absolute
    -1.0f <= LStick_N.X   <=     1.0f	// normalized

```


### Dead zones

To adjust dead zones sensitivity

```c#
int LStick_DeadZone    = 7849;
int RStick_DeadZone    = 8689;
int LTrigger_Threshold = 30;
int RTrigger_Threshold = 30;
```

_N methods return 0.0f when axis in a dead zone.


## Force feedback

Check presence of FFB

```c#
if (gamepad.FFB_Supported)
{
	// Already here
}
```

or

```c#
X.Gamepad.Capability caps = gamepad.Capabilities;

if ((caps.Flags & X.Gamepad.CapabilityFlags.FFB_Supported) == X.Gamepad.CapabilityFlags.FFB_Supported)
{
	// Already here
}
```


Stop vabrations

```c#
gamepad.FFB_Stop();
```


And starts again at left and right with strength lying beetwen 0.0f and 1.0f up to 100ms

```c#
gamepad.FFB_Vibrate(0.5f, 1.0f, 100);
```


Left motor only (low-frequency motor)

```c#
gamepad.FFB_LeftMotor(0.5f, 100);
```


Right side only (hi-frequency motor)

```c#
gamepad.FFB_RightMotor(0.5f, 100);
```
