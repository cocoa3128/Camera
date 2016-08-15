using AndroidCamera2 = Android.Hardware.Camera2;

namespace Camera {
	public class CameraDevice: AndroidCamera2.CameraDevice.StateCallback {
		public AndroidCamera2.CameraDevice mCamera;
		Camera2API mCamera2API;

		public CameraDevice(Camera2API api) {
			mCamera2API = api;

		}


		public override void OnOpened(AndroidCamera2.CameraDevice camera) {
			mCamera = camera;
			mCamera2API.CreateCaptureSettion();
		}

		public override void OnDisconnected(AndroidCamera2.CameraDevice camera) {
			camera.Close();
			mCamera = null;
		}

		public override void OnError(AndroidCamera2.CameraDevice camera, AndroidCamera2.CameraError error) {
			camera.Close();
			mCamera = null;
		}
	}
}

