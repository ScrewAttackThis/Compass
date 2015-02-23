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

        float azimuth;
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

                    //Temp compass for demo purposes
                    ImageView compassImageView = FindViewById<ImageView>(Resource.Id.compassImageView);

                    azimuth = lowPassFilter(orientation[0] * 180 / (float)Math.PI, azimuth);
                    compassImageView.Rotation = -azimuth; //Points in magnetic north.

                    var azimuthTextView = FindViewById<TextView>(Resource.Id.azimuthTextView);
                    azimuthTextView.Text = (-azimuth).ToString();
                }
            }
        }

        //Controls amount of smoothing.  0 < a < 1.  Smaller a, more smoothing but slower response
        const float ALPHA = 0.07f;
        private float lowPassFilter(float newInput, float output)
        {
            return (float)Math.Round(output + ALPHA * (newInput - output),1);
        }
    }
}

