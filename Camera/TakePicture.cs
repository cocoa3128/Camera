using System;

using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Hardware;

using AndroidCamera2 = Android.Hardware.Camera2;

namespace Camera {
	public class TakePicture:AndroidCamera2.CameraCaptureSession.CaptureCallback {
		public TakePicture() {
		}

		public override void OnCaptureStarted(AndroidCamera2.CameraCaptureSession session, AndroidCamera2.CaptureRequest request, long timestamp, long frameNumber) {
			base.OnCaptureStarted(session, request, timestamp, frameNumber);
		}

		public override void OnCaptureCompleted(AndroidCamera2.CameraCaptureSession session, AndroidCamera2.CaptureRequest request, AndroidCamera2.TotalCaptureResult result) {
			
		}

		public override void OnCaptureFailed(AndroidCamera2.CameraCaptureSession session, AndroidCamera2.CaptureRequest request, AndroidCamera2.CaptureFailure failure) {
			base.OnCaptureFailed(session, request, failure);
		}

		public override void OnCaptureProgressed(AndroidCamera2.CameraCaptureSession session, AndroidCamera2.CaptureRequest request, AndroidCamera2.CaptureResult partialResult) {
			base.OnCaptureProgressed(session, request, partialResult);
		}

		public override void OnCaptureSequenceAborted(AndroidCamera2.CameraCaptureSession session, int sequenceId) {
			base.OnCaptureSequenceAborted(session, sequenceId);
		}

		public override void OnCaptureSequenceCompleted(AndroidCamera2.CameraCaptureSession session, int sequenceId, long frameNumber) {
			base.OnCaptureSequenceCompleted(session, sequenceId, frameNumber);
		}
	}
}

