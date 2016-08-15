using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Hardware;

namespace Camera {
	public class OrientaionChange:Activity,ISensorEventListener{
		static int THRESHOLD_DEGREE {
			get {
				return 60;
			}
		}

		static int VERTICAL_TO_HORIZONTAL_DEGREE {
			get {
				return THRESHOLD_DEGREE;
			}
		}

		static int HORIZONTAL_TO_VERTICAL_DEGREE {
			get {
				return 90 - THRESHOLD_DEGREE;
			}
		}

		static double RAD2DEG {
			get {
				return 180 / Math.PI;
			}
		}

		static int MATRIX_SIZE{
			get{
				return 16;
			}
		}


		enum Orientation {
			OrientationNotDetected = -1,
			OrientationVertical = 0,
			OrientationHorizontal,
			OrientationHorizontalReversed
		};


		Orientation PreScreenOrientaion = Orientation.OrientationNotDetected;
		float[] m_MagneticValues = new float[3];
		float[] m_AccelerometerValues = new float[3];
		float[] m_OrientationValues = new float[3];

		float[] inR = new float[MATRIX_SIZE];
		float[] outR = new float[MATRIX_SIZE];
		float[] I = new float[MATRIX_SIZE];


		Context m_Context;

		public OrientaionChange(Context context){
			m_Context = context;
		}


		public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy) {

		}

		public void OnSensorChanged(SensorEvent e) {
			switch(e.Sensor.Type){
				case SensorType.MagneticField:
					e.Values.CopyTo(m_MagneticValues, 0);
					break;
				case SensorType.Accelerometer:
					e.Values.CopyTo(m_AccelerometerValues, 0);
					break;
			}

			if ((m_MagneticValues == null) || (m_AccelerometerValues == null))
				return;

			SensorManager.GetRotationMatrix(inR, I, m_AccelerometerValues, m_MagneticValues);
			SensorManager.RemapCoordinateSystem(inR, Android.Hardware.Axis.X, Android.Hardware.Axis.Y, outR);
			SensorManager.GetOrientation(outR, m_OrientationValues);

			int azimuth = (int)(m_OrientationValues[0] * RAD2DEG);
			int pitch = (int)(m_OrientationValues[1] * RAD2DEG);
			int roll = (int)(m_OrientationValues[2] * RAD2DEG);

			int absRoll = Math.Abs(roll);
			if(PreScreenOrientaion == Orientation.OrientationNotDetected){
				PreScreenOrientaion = (absRoll < VERTICAL_TO_HORIZONTAL_DEGREE ? 
				                       Orientation.OrientationVertical : Orientation.OrientationHorizontal);
			}else if(absRoll < 90){
				PreScreenOrientaion = GetOrientation(PreScreenOrientaion, roll);
			}else{
				int PlusMinus = (roll >= 0 ? 1 : -1);
				roll = (absRoll - 90) * PlusMinus;
				var tmp = InvertOrientaion(PreScreenOrientaion);
				PreScreenOrientaion = InvertOrientaion(GetOrientation(tmp, roll));
			}

			Console.WriteLine("角度:{0}     向き:{1}", roll, PreScreenOrientaion);
		}

		Orientation InvertOrientaion(Orientation orientation){
			Orientation ReturnValue = Orientation.OrientationNotDetected;

			if(orientation == Orientation.OrientationHorizontal){
				ReturnValue = Orientation.OrientationVertical;
			}else if(orientation == Orientation.OrientationVertical){
				ReturnValue = Orientation.OrientationHorizontal;
			}

			return ReturnValue;
		}

		Orientation GetOrientation(Orientation pre, int roll){
			int absRoll = Math.Abs(roll);
			Orientation returnValue = Orientation.OrientationNotDetected;

			if(pre == Orientation.OrientationVertical){
				returnValue = (absRoll < VERTICAL_TO_HORIZONTAL_DEGREE ? 
				               Orientation.OrientationVertical : Orientation.OrientationHorizontal);
			}else if(pre == Orientation.OrientationHorizontal){
				returnValue = (absRoll < HORIZONTAL_TO_VERTICAL_DEGREE ? 
				               Orientation.OrientationVertical : Orientation.OrientationHorizontal);
			}

			return returnValue;
		}
	}
}

