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
        
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            sensorMgr = (SensorManager)GetSystemService(Context.SensorService);
            accelerometer = sensorMgr.GetDefaultSensor(SensorType.Accelerometer);
            magnetometer = sensorMgr.GetDefaultSensor(SensorType.MagneticField);
        }

        protected override void OnResume()
        {
            base.OnResume();


            if (magnetometer != null && accelerometer != null)
            {
                sensorMgr.RegisterListener(this, accelerometer, SensorDelay.Ui);
                sensorMgr.RegisterListener(this, magnetometer, SensorDelay.Ui);
            }
            else
            {
                //Doesn't have required sensors
            }
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
        Queue<float> azimuths = new Queue<float>();

        public void OnSensorChanged(SensorEvent e)
        {
            if (e.Sensor.Type == SensorType.MagneticField)
            {
                magnet.Clear();
                magnet.AddRange(e.Values);
            }
            if (e.Sensor.Type == SensorType.Accelerometer)
            {
                gravity.Clear();
                gravity.AddRange(e.Values);
            }

            if(magnet.Count > 0 && gravity.Count > 0)
            {
                float[] R = new float[9];
                float[] I = new float[9];
                bool worked = SensorManager.GetRotationMatrix(R,I,gravity.ToArray(),magnet.ToArray());

                if(worked)
                {
                    float[] orientation = new float[3];
                    SensorManager.GetOrientation(R, orientation);
                    
                    float azimuth = orientation[0] * 180 / (float)Math.PI; //convert to degrees for magnetic north.

                    //Temp compass for demo purposes
                    ImageView compassImageView = FindViewById<ImageView>(Resource.Id.compassImageView);
                    compassImageView.Rotation = -runningAverage(azimuth); //Points in magnetic north.
                }
            }
        }

        private float runningAverage(float newValue)
        {
            if (azimuths.Count > 35)
            {
                azimuths.Dequeue();
            }

            azimuths.Enqueue((float)Math.Round(newValue));
            float average = 0;

            foreach(float value in azimuths)
            {
                average += value;
            }

            return average/azimuths.Count;
        }
    }
}

