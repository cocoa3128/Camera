using System;

using AndroidCamera2 = Android.Hardware.Camera2;


namespace Camera {
	public class CameraCapture: AndroidCamera2.CameraCaptureSession.StateCallback {
		//public static AndroidCamera2.CameraCaptureSession mSession;

		public CameraCapture() {
		}

		public Action<AndroidCamera2.CameraCaptureSession> OnConfiguredAction;
		public override void OnConfigured(AndroidCamera2.CameraCaptureSession session) {
			if(OnConfiguredAction != null){
				OnConfiguredAction(session);
			}
		}
		public Action<AndroidCamera2.CameraCaptureSession> OnConfigueFailedAction;
		public override void OnConfigureFailed(AndroidCamera2.CameraCaptureSession session) {
			if (OnConfigueFailedAction != null)
				OnConfigueFailedAction(session);
		}


	}
}

