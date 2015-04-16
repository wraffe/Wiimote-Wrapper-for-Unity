using UnityEngine;
using System.Collections;

public class WiiReader : MonoBehaviour {

	private ClientWiiState wiiState;
	private WiiUnity_Client client;
	private bool connected;
	
	public GUIText aField, bField, upField, downField, leftField, rightField; 
	public GUIText oneField, twoField, plusField, minusField, homeField;
	public GUIText xAccelField, yAccelField, zAccelField; 
	public GUIText ncCField, ncZField, ncJoystickXField, ncJoystickYField;
	public GUIText ncXAccelField, ncYAccelField, ncZAccelField;
	public GUIText rumbleField, irActiveField, irVisibleField;
	public GUIText irMidpointField, ir1Field, ir2Field;
	
	
	
	// Use this for initialization
	void Start () {
		client = new WiiUnity_Client();
        connected = client.StartClient();
	}
	

	void LateUpdate () {
		if (connected)
		{
			client.UpdateButtons(1);
			client.UpdateAccel(1);
			client.UpdateNunchuck(1);
			if (wiiState != null)
				if (wiiState.irActive)
					client.UpdateIR(1);			
			
			if (Input.GetKeyDown(KeyCode.Space))
				client.ToggleRumble(1);
			if (Input.GetKeyDown(KeyCode.LeftShift))
			{
				Debug.Log("Left Shift pressed");
				client.ToggleIR(1);
			}
				
			wiiState = client.GetWiiState(1);
				
		
			// Get gesture (posture) of wiimote on button A press. Take snap shot and decipher it
			if (wiiState.A) { 
				// Pointing up: -0.25<x<0.25,  y<-0.85, -0.25<z<0.25
				// parallel to ground - facing up: -0.1<x<0.1, -0.25<y<0.25, 0.90<z
				// parallel to ground - rolled left: x<-0.85, -0.25<y<0.25, -0.25<z<0.25 	
				if (wiiState.accelX >= -0.25 && wiiState.accelX <= 0.25
					&& wiiState.accelY <= -0.85
					&& wiiState.accelZ >= -0.25 && wiiState.accelZ <= 0.25)
				{
					Debug.Log("Pointing up");				
				}
				else if (wiiState.accelX >= -0.1 && wiiState.accelX <= 0.1
					&& wiiState.accelY >= -0.25 && wiiState.accelY <= 0.25
					&& wiiState.accelZ >= 0.90)
				{
					Debug.Log("Flat");
				}
				else if (wiiState.accelX <= -0.85
					&& wiiState.accelY >= -0.25 && wiiState.accelY <= 0.25
					&& wiiState.accelZ >= -0.25 && wiiState.accelZ <= 0.25)
				{
					Debug.Log("Side");					
				}
			}			
		}
	}
	
	void OnGUI() {	
		if (connected)
		{
			aField.text = wiiState.A.ToString(); bField.text = wiiState.B.ToString();
			upField.text = wiiState.Up.ToString(); downField.text = wiiState.Down.ToString();
			leftField.text = wiiState.Left.ToString(); rightField.text = wiiState.Right.ToString();
			oneField.text = wiiState.One.ToString(); twoField.text = wiiState.Two.ToString();
			plusField.text = wiiState.Plus.ToString(); minusField.text = wiiState.Minus.ToString();
			homeField.text = wiiState.Home.ToString();
			
			xAccelField.text = wiiState.accelX.ToString(); 
			yAccelField.text = wiiState.accelY.ToString();
			zAccelField.text = wiiState.accelZ.ToString();
			
			ncCField.text = wiiState.ncC.ToString();
			ncZField.text = wiiState.ncZ.ToString();
			ncJoystickXField.text = wiiState.ncJoyX.ToString();
			ncJoystickYField.text = wiiState.ncJoyY.ToString();
			
			ncXAccelField.text = wiiState.ncAccelX.ToString();
			ncYAccelField.text = wiiState.ncAccelY.ToString();
			ncZAccelField.text = wiiState.ncAccelZ.ToString();
			
			rumbleField.text = wiiState.Rumble.ToString();
			
			irActiveField.text = wiiState.irActive.ToString();
			irVisibleField.text = wiiState.irVisible.ToString();
			irMidpointField.text = "( " + wiiState.irMidpointX + " , " + wiiState.irMidpointY + " )";
			ir1Field.text = "( " + wiiState.ir1PosX + " , " + wiiState.ir1PosY + " )";
			ir2Field.text = "( " + wiiState.ir2PosX + " , " + wiiState.ir2PosY + " )";
		}
	}
	
	void OnApplicationQuit() {
		if (connected)
			client.EndClient();
	}
}
