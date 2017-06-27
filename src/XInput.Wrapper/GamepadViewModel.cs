using System;
using System.ComponentModel;

namespace XInput.Wrapper
{
    public class GamepadViewModel : INotifyPropertyChanged
    {
        protected X.Gamepad gamepad;
        public X.Gamepad Gamepad { get { return gamepad; } }

        bool ButtonX { get; set; }
        bool ButtonY { get; set; }
        bool ButtonA { get; set; }
        bool ButtonB { get; set; }

        public GamepadViewModel(X.Gamepad xGamepad)
        {
            gamepad = xGamepad;
        }

        private void Gamepad_StateChanged(object sender, EventArgs e)
        {
            ButtonA = TestButton("ButtonA", gamepad.A_down, gamepad.A_up);
            ButtonB = TestButton("ButtonB", gamepad.B_down, gamepad.B_up);
            ButtonX = TestButton("ButtonX", gamepad.X_down, gamepad.X_up);
            ButtonY = TestButton("ButtonY", gamepad.Y_down, gamepad.Y_up);
        }

        private bool TestButton(string propertyName, bool gamepadDown, bool gamepadUp)
        {
            if (gamepadDown || gamepadUp)
                OnPropertyChanged(propertyName);

            return gamepadDown;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler pceh = PropertyChanged;
            pceh?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    } // class
}
