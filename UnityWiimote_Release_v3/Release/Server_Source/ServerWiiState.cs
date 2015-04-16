using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WiimoteLib;

// A wrapper class for the WiimoteLib dll
class ServerWiiState
{
    // The wiimote itself
    private Wiimote wm;

    // All the button states of the wiimote. True if button is pressed down, false if released
    private bool A, B, Up, Down, Left, Right, Plus, Minus, One, Two, Home;

    private bool Rumble;

    // The accelerometer states
    private float accelX, accelY, accelZ;

    // Infrared states. Floats between (0,1)
    public bool irActive, irVisible;
    public float irMidpointX, irMidpointY, ir1PosX, ir1PosY, ir2PosX, ir2PosY;

 
    // Nunchuck buttons and joystick
    public bool ncC, ncZ;
    public float ncJoyX, ncJoyY;

    // Nunchuck accelerometer states
    public float ncAccelX, ncAccelY, ncAccelZ;

    // Nunchuck accelerometer states
    public float ncAccelX, ncAccelY, ncAccelZ;
        

    public ServerWiiState(Wiimote newWM)
    {
        // create a new instance of the Wiimote    
        wm = newWM;

        // default all values
        A = false; B = false; Up = false; Down = false; Left = false; Right = false;
        Plus = false; Minus = false; One = false; Two = false; Home = false;

        accelX = (float)0.0; accelY = (float)0.0; accelZ = (float)0.0;

        irActive = false; irVisible = false;
        irMidpointX = (float)0.0; irMidpointY = (float)0.0; ir1PosX = (float)0.0; 
        ir1PosY = (float)0.0; ir2PosX = (float)0.0; ir2PosY = (float)0.0;        

        ncC = false; ncZ = false;
        ncJoyX = (float)0.0; ncJoyY = (float)0.0;

        ncAccelX = (float)0.0; ncAccelY = (float)0.0; ncAccelX = (float)0.0;
    }

    public bool InitWiimote(int wmID)
    {
        // connect to the Wiimote 
        try
        {
            wm.Connect();
        }
        catch (Exception e)
        {
            // If no wiimote is connected
            Console.WriteLine(e.Message);
            Console.WriteLine("A Wiimote was found but couldn't be connected to");
            return false;
        }

        // Stop the LEDs from blinking
        wm.SetLEDs(wmID+1);
        // set the report type to return the IR sensor, accelerometer data, and buttons    
        wm.SetReportType(InputReport.ExtensionAccel, true);

        UpdateButtons();
        UpdateAccel();
        UpdateNunchuck();

        return true;       
    }

    public void Disconnect()
    {
        try
        {
            wm.Disconnect();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }


    // Update methods to get data from wiimote through WiimoteLib
    public void UpdateButtons()
    {
        // Get the state of the buttons, accelerometer, and IR from the wiimote itself
        WiimoteState ws = wm.WiimoteState;

        // Assign class button states
        A = ws.ButtonState.A; B = ws.ButtonState.B; Up = ws.ButtonState.Up;
        Down = ws.ButtonState.Down; Left = ws.ButtonState.Left; 
        Right = ws.ButtonState.Right; Plus = ws.ButtonState.Plus; 
        Minus = ws.ButtonState.Minus; One = ws.ButtonState.One;
        Two = ws.ButtonState.Two; Home = ws.ButtonState.Home;
    }

    public void UpdateAccel()
    {
        WiimoteState ws = wm.WiimoteState;

        // Assign accelerometer values
        accelX = ws.AccelState.Values.X;
        accelY = ws.AccelState.Values.Y;
        accelZ = ws.AccelState.Values.Z;
    }

    public void UpdateNunchuck()
    {
        WiimoteState ws = wm.WiimoteState;

        ncC = ws.NunchukState.C; ncZ = ws.NunchukState.Z;
        ncJoyX = ws.NunchukState.Joystick.X;
        ncJoyY = ws.NunchukState.Joystick.Y;
        ncAccelX = ws.NunchuckState.AccelState.Values.X;
        ncAccelY = ws.NunchuckState.AccelState.Values.Y;
        ncAccelZ = ws.NunchuckState.AccelState.Values.Z;
    }

    public void UpdateIR()
    {
        WiimoteState ws = wm.WiimoteState;

        if (!ws.IRState.IRSensors[0].Found || !ws.IRState.IRSensors[1].Found)
            irVisible = false;
        else
        {
            irVisible = true;
            irMidpointX = ws.IRState.Midpoint.X; irMidpointY = ws.IRState.Midpoint.Y;
            ir1PosX = ws.IRState.IRSensors[0].Position.X; ir1PosY = ws.IRState.IRSensors[0].Position.Y;
            ir2PosX = ws.IRState.IRSensors[1].Position.X; ir2PosY = ws.IRState.IRSensors[1].Position.Y;
        }
    }

    public void ToggleRumble()
    {
        Rumble = !Rumble;
        wm.SetRumble(Rumble);
    }

    public void ToggleIR()
    {
        if (irActive)
        {
            wm.SetReportType(InputReport.IRExtensionAccel, true);
            wm.IRState.Mode = IRMode.Basic;
            irActive = true;
            UpdateIR();
        }
        else
        {
            wm.SetReportType(InputReport.ExtensionAccel, true);
            wm.IRState.Mode = IRMode.Off;
            irActive = false; irVisible = false;
            irMidpointX = (float)0.0; irMidpointY = (float)0.0; ir1PosX = (float)0.0;
            ir1PosY = (float)0.0; ir2PosX = (float)0.0; ir2PosY = (float)0.0;    
        }
    }


    // Converting state data to a byte string to send to the client
    public byte[] ButtonsToBytes()
    {
        String sStates = A + " " + B + " " + Up + " " + Down + " " + Left + " "
                        + Right + " " + Plus + " " + Minus + " " + One + " "
                        + Two + " " + Home + " ";
        return Encoding.ASCII.GetBytes(sStates);
    }

    public byte[] AccelToBytes()
    {
        String sStates = accelX + " " + accelY + " " + accelZ + " ";
        return Encoding.ASCII.GetBytes(sStates);
    }

    public byte[] IRToBytes()
    {
        String sStates = irActive + " " + irVisible + " " + irMidpointX + " " + irMidpointY + " " 
                        + ir1PosX + " " + ir1PosY + " " + ir2PosX + " " + ir2PosY + " ";
        return Encoding.ASCII.GetBytes(sStates);
    }

    public byte[] NunchuckToBytes()
    {
        String sStates = ncC + " " + ncZ + " " + ncJoyX + " " + ncJoyY + " " + ncAccelX + " " + ncAccelY + " " + ncAccelZ + " ";
        return Encoding.ASCII.GetBytes(sStates);
    }

    public byte[] RumbleToBytes()
    {
        String sState = Rumble.ToString();
        return Encoding.ASCII.GetBytes(sStates);
    }
}

