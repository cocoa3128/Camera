using System;
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

using AndroidCamera = Android.Hardware.Camera;
using AndroidCamera2 = Android.Hardware.Camera2;
using Android.Hardware.Camera2.Params;
using Console = System.Console;
using Environment = Android.OS.Environment;
using Matrix = Android.Graphics.Matrix;

namespace Camera {
	public class CameraDevice: AndroidCamera2.CameraDevice.StateCallback {
		public AndroidCamera2.CameraDevice m_CameraDevice;
		Camera2API m_Camera2API;

		public CameraDevice(Camera2API api) {
			m_Camera2API = api;

		}


		public override void OnOpened(AndroidCamera2.CameraDevice camera) {
			m_CameraDevice = camera;
			m_Camera2API.CreateCaptureSettion();
		}

		public override void OnDisconnected(AndroidCamera2.CameraDevice camera) {
			camera.Close();
			m_CameraDevice = null;
		}

		public override void OnError(AndroidCamera2.CameraDevice camera, AndroidCamera2.CameraError error) {
			camera.Close();
			m_CameraDevice = null;
		}
	}
}

