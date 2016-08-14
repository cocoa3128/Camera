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
using System;


namespace Camera {
	public class CameraCapture: AndroidCamera2.CameraCaptureSession.StateCallback {
		public AndroidCamera2.CameraCaptureSession m_PreviewSession;
		Camera2API m_Camea2API;

		public CameraCapture(Camera2API api) {
			m_Camea2API = api;
		}

		public override void OnConfigured(AndroidCamera2.CameraCaptureSession session) {
			m_PreviewSession = session;
			m_Camea2API.UpdatePreview();
		}

		public override void OnConfigureFailed(AndroidCamera2.CameraCaptureSession session) {
			
		}

	}
}

