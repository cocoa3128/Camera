using System;

using AndroidCamera2 = Android.Hardware.Camera2;

namespace Camera {
	public class CameraDevice: AndroidCamera2.CameraDevice.StateCallback {

		public CameraDevice() {

		}

		public Action<AndroidCamera2.CameraDevice> OnOpenedAction;
		public override void OnOpened(AndroidCamera2.CameraDevice camera) {
			if (OnOpenedAction != null)
				OnOpenedAction(camera);
		}

		public override void OnDisconnected(AndroidCamera2.CameraDevice camera) {
			camera.Close();
			//mCamera = null;
		}

		public override void OnError(AndroidCamera2.CameraDevice camera, AndroidCamera2.CameraError error) {
			camera.Close();
			//mCamera = null;
		}
	}
}

