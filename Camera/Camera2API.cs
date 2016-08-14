using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Android.App;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Locations;
using Android.Content;
using Android.Opengl;
using Android.Util;
using Android.Runtime;


using AndroidCamera = Android.Hardware.Camera;
using AndroidCamera2 = Android.Hardware.Camera2;
using Android.Hardware.Camera2.Params;
using Console = System.Console;
using Environment = Android.OS.Environment;
using Matrix = Android.Graphics.Matrix;

using System;

namespace Camera {

	public class Camera2API {
		public enum CameraRotation{
			ROTATION_0,
			ROTATION_90,
			ROTATION_180,
			ROTATION_270,
		}

		struct Vector<Type> {
			
			public Type X { set; get; }
			public Type Y { set; get; }

			public Vector(Type x, Type y){
				X = x;
				Y = y;
			}
		};


		Camera2API m_Camera2API;

		public AndroidCamera2.CameraManager m_CameraManager;
		Context m_Context;
		CameraDevice m_CameraDevice;
		CameraCapture m_PreviewSession;
		CameraRotation m_CameraRotation;
		AndroidCamera2.CaptureRequest.Builder m_PreviewBuilder;
		Size m_CameraSize;
		TextureView m_TextureView;
		Display display;
		Vector<int> DisplaySize;


		public Camera2API(Context context, TextureView texture)  {
			m_Context = context;
			m_TextureView = texture;
			m_Camera2API = this;
			m_CameraDevice = new CameraDevice(this);
			m_PreviewSession = new CameraCapture(this);
			m_CameraManager = (AndroidCamera2.CameraManager)m_Context.GetSystemService(Context.CameraService);


			IWindowManager WindowManager = m_Context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
			display = WindowManager.DefaultDisplay;

		}

		public void OpenCamera(AndroidCamera2.LensFacing facing) {
			try {
				foreach (var cameraId in m_CameraManager.GetCameraIdList()) {
					
					AndroidCamera2.CameraCharacteristics m_Characteristics = m_CameraManager.GetCameraCharacteristics(cameraId);
					if (m_Characteristics.Get(AndroidCamera2.CameraCharacteristics.LensFacing).Equals((AndroidCamera2.LensFacing.Back).CompareTo(facing))) {
						StreamConfigurationMap map = (StreamConfigurationMap)m_Characteristics.Get(AndroidCamera2.CameraCharacteristics.ScalerStreamConfigurationMap);
						m_CameraSize = map.GetOutputSizes(Java.Lang.Class.FromType(typeof(SurfaceTexture)))[0];
						m_CameraManager.OpenCamera(cameraId, m_CameraDevice, null);

						return;
					}

				}
			}catch(AndroidCamera2.CameraAccessException e){
				Log.Debug("{0}",e.ToString());
			}
		}

		public void CloseCamera(){
			m_CameraDevice.m_CameraDevice.Close();
			m_CameraManager.UnregisterFromRuntime();
		}

		public void CreateCaptureSettion(){
			if (!m_TextureView.IsAvailable)
				return;


			var rotation = 0;
			Vector<float> Scale = new Vector<float>(0, 0);
			DetectCameraRotation();
			switch(m_CameraRotation){
				case CameraRotation.ROTATION_0:
					rotation = 0;
					Scale.X = 1;// (float)m_CameraSize.Height / m_CameraSize.Width;
					Scale.Y = 1;//(float)m_CameraSize.Width / m_CameraSize.Height;
					break;
				case CameraRotation.ROTATION_90:
					rotation = 270;
					Scale.X = (float)m_CameraSize.Width / m_CameraSize.Height;
					Scale.Y = (float)m_CameraSize.Height / m_CameraSize.Width;
					break;
				case CameraRotation.ROTATION_180:
					rotation = 180;
					Scale.X = 1;//(float)m_CameraSize.Height / m_CameraSize.Width;
					Scale.Y = 1;//(float)m_CameraSize.Width / m_CameraSize.Height;
					break;
				case CameraRotation.ROTATION_270:
					rotation = 90;
					Scale.X = (float)m_CameraSize.Width / m_CameraSize.Height;
					Scale.Y = (float)m_CameraSize.Height / m_CameraSize.Width;
					break;
			}

			Matrix matrix = new Matrix();
			matrix.PostRotate(rotation, m_CameraSize.Width / 2, m_CameraSize.Height / 2);
			matrix.PostScale(Scale.X,
			                 Scale.Y, 
			                 m_CameraSize.Width / 2,
			                 m_CameraSize.Height / 2);
			m_TextureView.SetTransform(matrix);
			Toast.MakeText(m_Context, "Width:" + m_TextureView.Width +
						   "\nHeight:" + m_TextureView.Height,
						   ToastLength.Short).Show();
			SurfaceTexture texture = m_TextureView.SurfaceTexture;
			texture.SetDefaultBufferSize(m_CameraSize.Width, m_CameraSize.Height);
			Surface surface = new Surface(texture);


			try{
				m_PreviewBuilder = m_CameraDevice.m_CameraDevice.CreateCaptureRequest(AndroidCamera2.CameraTemplate.Preview);

			}catch(AndroidCamera2.CameraAccessException e){
				Log.Debug("{0}", e.ToString());
			}

			m_PreviewBuilder.AddTarget(surface);
			var list = new List<Surface>();
			list.Add(surface);
			try{
				m_CameraDevice.m_CameraDevice.CreateCaptureSession(list, m_PreviewSession, null);
			}catch(AndroidCamera2.CameraAccessException e){
				Log.Debug("{0}", e.ToString());
			}
		}

		public void UpdatePreview(){
			m_PreviewBuilder.Set(AndroidCamera2.CaptureRequest.ControlAfMode, 4);
			HandlerThread thread = new HandlerThread("CameraPreview");
			thread.Start();
			Handler BGHandler = new Handler(thread.Looper);

			try{
				m_PreviewSession.m_PreviewSession.SetRepeatingRequest(m_PreviewBuilder.Build(), null, BGHandler);
			}catch(AndroidCamera2.CameraAccessException e){
				Log.Debug("{0}", e.ToString());
			}
		}

		public void SetCameraRotation(){

		}



		// 画面のサイズを取得する
		void GetDisplaySize() {
			Point point = new Point(0, 0);
			display.GetRealSize(point);

			if (point.X < point.Y) {
				DisplaySize.X = point.X;
				DisplaySize.Y = point.Y;
			} else {
				DisplaySize.X = point.Y;
				DisplaySize.Y = point.X;
			}
		}

		[Obsolete]
		void DetectCameraRotation(){
			var rotation = display.Rotation;

			switch(rotation){
				case SurfaceOrientation.Rotation0:
					m_CameraRotation = CameraRotation.ROTATION_0;
					break;
				case SurfaceOrientation.Rotation90:
					m_CameraRotation = CameraRotation.ROTATION_90;
					break;
				case SurfaceOrientation.Rotation180:
					m_CameraRotation = CameraRotation.ROTATION_180;
					break;
				case SurfaceOrientation.Rotation270:
					m_CameraRotation = CameraRotation.ROTATION_270;
					break;
			}
		}

		public void OnOrientationChanged() {
			Toast.MakeText(m_Context, "Rotation Changed", ToastLength.Short).Show();
			CreateCaptureSettion();
			//throw new NotImplementedException();
		}
	}
}

