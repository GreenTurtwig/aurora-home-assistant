using Aurora;
using Aurora.Devices;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using RestSharp;

public class HomeAssistantLEDDevice {
    public bool enabled = true; //Switch to True, to enable it in Aurora
    
    private Color device_color = Color.Black;

    private RestClient client;

    // SETTINGS FOR HOME ASSISTANT RGB LIGHT, ONLY CHANGE THESE! //
    public string devicename = ""; // Name that will show up in Aurora.      e.g. "Desk Lights"
    string haURL = "";             // URL of Home Assistant instance.        e.g. "http://192.168.0.24:8123"
    string haPassword = "";        // Password (if any) for Home Assistant.
    string haLEDID = "";           // Name of the RGB lights to control.     e.g. "desk_lights"


    public bool Initialize() {
        try {
            //Perform necessary actions to initialize your device
            client = new RestClient();
            client.BaseUrl = new Uri(haURL + "/api/services/light");

            ChangeState(false);
            return true;
        }
        catch(Exception exc) {
            return false;
        }
    }
    
    public void Reset() {
        //Perform necessary actions to reset your device
    }
    
    public void Shutdown() {
        //Perform necessary actions to shutdown your device
        ChangeState(true);
    }
    
    public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, bool forced) {
        try {
            foreach (KeyValuePair<DeviceKeys, Color> key in keyColors) {
                //Iterate over each key and color and send them to your device
                
                if (key.Key == DeviceKeys.ESC) {
                    //For example if we're basing our device color on Peripheral colors
                    SendColorToDevice(key.Value, forced);
                }
            }
            
            return true;
        }
        catch(Exception exc) {
            return false;
        }
    }
    
    //Custom method to send the color to the device
    private void SendColorToDevice(Color color, bool forced) {
        //Check if device's current color is the same, no need to update if they are the same
        if (!device_color.Equals(color) || forced) {
            //NOTE: Do not have any logging during color set for performance reasons. Only use logging for debugging
            //Global.logger.LogLine(string.Format("[C# Script] Sent a color, {0} to the device", color));
            
            //Update device colour in Home Assistant.
            ChangeColour(color.R, color.G, color.B);

            //Update device color locally
            device_color = color;
        }
    }

    public void ChangeState(bool isOn) {
        var request = new RestRequest();

        if (isOn == true) {
           request.Resource = ("/turn_off?api_password=" + haPassword);
        } else {
           request.Resource = ("/turn_on?api_password=" + haPassword);
        }

        request.Method = Method.POST;
        request.RequestFormat = DataFormat.Json;
        request.AddBody(new { entity_id = "light." + haLEDID });

        client.Execute(request);
    }

    public void ChangeColour(int red, int green, int blue) {
        int[] rgb = { red, green, blue };

        var request = new RestRequest();
        request.Resource = ("/turn_on?api_password=" + haPassword);
        request.Method = Method.POST;
        request.RequestFormat = DataFormat.Json;
        request.AddBody(new { entity_id = "light." + haLEDID, rgb_color = rgb });

        client.Execute(request);
    }
}