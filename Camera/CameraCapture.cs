using AndroidCamera2 = Android.Hardware.Camera2;


namespace Camera {
	public class CameraCapture: AndroidCamera2.CameraCaptureSession.StateCallback {
		public AndroidCamera2.CameraCaptureSession mSession;
		Camera2API mCamea2API;

		public CameraCapture(Camera2API api) {
			mCamea2API = api;
		}

		public override void OnConfigured(AndroidCamera2.CameraCaptureSession session) {
			mSession = session;
			mCamea2API.UpdatePreview();
		}

		public override void OnConfigureFailed(AndroidCamera2.CameraCaptureSession session) {
			
		}

	}
}

