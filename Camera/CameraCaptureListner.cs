using System;

using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Hardware;

using AndroidCamera2 = Android.Hardware.Camera2;

namespace Camera {
	public class CameraCaptureListner:AndroidCamera2.CameraCaptureSession.CaptureCallback {
		public CameraCaptureListner() {
		}

		public override void OnCaptureStarted(AndroidCamera2.CameraCaptureSession session, AndroidCamera2.CaptureRequest request, long timestamp, long frameNumber) {
			base.OnCaptureStarted(session, request, timestamp, frameNumber);
		}

		public Action<AndroidCamera2.CameraCaptureSession, AndroidCamera2.CaptureRequest, AndroidCamera2.TotalCaptureResult> OnCaptureCompletedAction;
		public override void OnCaptureCompleted(AndroidCamera2.CameraCaptureSession session, AndroidCamera2.CaptureRequest request, AndroidCamera2.TotalCaptureResult result) {
			if (OnCaptureCompletedAction != null)
				OnCaptureCompletedAction(session, request, result);
			OnCaptureCompletedAction = null;
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

