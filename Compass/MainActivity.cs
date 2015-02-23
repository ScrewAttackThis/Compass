using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Content.PM;
using Android.Hardware;
using System.Collections.Generic;

namespace Compass
{
    [Activity(Label = "Compass", MainLauncher = true, Icon = "@drawable/icon", ScreenOrientation=ScreenOrientation.Portrait)]
    public class MainActivity : Activity, ISensorEventListener
    {
        SensorManager sensorMgr;
        Sensor accelerometer;
        Sensor magnetometer;

        float azimuth;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            sensorMgr = (SensorManager)GetSystemService(Context.SensorService);
            accelerometer = sensorMgr.GetDefaultSensor(SensorType.Accelerometer);
            magnetometer = sensorMgr.GetDefaultSensor(SensorType.MagneticField);

            if(magnetometer != null && accelerometer != null)
            {
                
            }
            else
            {
                //Doesn't have required sensors
            }
        }

        protected override void OnResume()
        {
            base.OnResume();

            sensorMgr.RegisterListener(this, accelerometer, SensorDelay.Ui);
            sensorMgr.RegisterListener(this, magnetometer, SensorDelay.Ui);
        }

        protected override void OnPause()
        {
            base.OnPause();
            sensorMgr.UnregisterListener(this);
        }

        public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
        {

        }

        List<float> gravity = new List<float>();
        List<float> magnet = new List<float>();

        public void OnSensorChanged(SensorEvent e)
        {
            if (e.Sensor.Type == SensorType.MagneticField)
                magnet.AddRange(e.Values);
            if (e.Sensor.Type == SensorType.Accelerometer)
                gravity.AddRange(e.Values);

            if(magnet.Count > 0 && gravity.Count > 0)
            {
                float[] R = new float[9];
                float[] I = new float[9];
                bool worked = SensorManager.GetRotationMatrix(R,I,gravity.ToArray(),magnet.ToArray());

                if(worked)
                {
                    float[] orientation = new float[3];
                    SensorManager.GetOrientation(R, orientation);

                    azimuth = orientation[0];
                }
            }
        }
    }
}

