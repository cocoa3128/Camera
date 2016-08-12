using Android.App;
using Android.Widget;
using Android.OS;

using Android.Hardware;
using Android.Hardware.Camera2;
using Android.Hardware.Camera2.Params;
using Android.Views;
using Android.Media;

using Android.Util;

using System;
using System.IO;
using Java.IO;

using AndroidCamera = Android.Hardware.Camera;
using Environment = Android.OS.Environment;
using Console = System.Console;

namespace Camera {
	[Activity(
		Label = "Camera",
		MainLauncher = true,
		Icon = "@mipmap/icon",
		HardwareAccelerated = true
	)]
	[Obsolete]
	public class MainActivity : Activity, TextureView.ISurfaceTextureListener, AndroidCamera.IPictureCallback, AndroidCamera.IAutoFocusCallback {

		public AndroidCamera m_Camera;
		TextureView m_TextureView;
		public static Java.IO.File FileDir;
		bool isTakeEnabled = false;

		protected override void OnCreate(Bundle savedInstanceState) {
			base.OnCreate(savedInstanceState);

			FileDir = Application.FilesDir;

			Window.AddFlags(WindowManagerFlags.Fullscreen);
			RequestWindowFeature(WindowFeatures.NoTitle);

			FrameLayout rootView = new FrameLayout(ApplicationContext);
			SetContentView(rootView);

			m_TextureView = new TextureView(ApplicationContext);
			m_TextureView.SurfaceTextureListener = this;

			rootView.AddView(m_TextureView);

			Button button = new Button(ApplicationContext);
			button.Text = "撮影";
			button.LayoutParameters = new ViewGroup.LayoutParams(200, 150);
			button.Clickable = true;

			button.Touch += (sender, e) => {
				if (e.Event.Action == MotionEventActions.Down){
					isTakeEnabled = false;
					m_Camera.AutoFocus(this);
				}

				if(e.Event.Action == MotionEventActions.Up) {
					if ((e.Event.GetX() > button.GetX()) &&
					    (e.Event.GetX() < (button.GetX() + button.Width)) &&
					   (e.Event.GetY() > button.GetY()) &&
					    (e.Event.GetY() < (button.GetY() + button.Height))) {
						isTakeEnabled = true;
						m_Camera.AutoFocus(this);
					}
				}
			};

			rootView.AddView(button);
		}

		public void OnSurfaceTextureAvailable(Android.Graphics.SurfaceTexture surface, int w, int h) {
			m_Camera = AndroidCamera.Open();
			m_TextureView.LayoutParameters = new FrameLayout.LayoutParams(w, h);

			DetectScreenOrientation();

			try {
				m_Camera.SetPreviewTexture(surface);
				m_Camera.StartPreview();
			} catch (Java.IO.IOException e) {
				System.Console.WriteLine(e.Message);
			}
		}

		public bool OnSurfaceTextureDestroyed(Android.Graphics.SurfaceTexture surface) {
			m_Camera.StopPreview();
			m_Camera.Release();

			return true;
		}

		public void OnSurfaceTextureUpdated(Android.Graphics.SurfaceTexture surface) {
		}

		public void OnSurfaceTextureSizeChanged(Android.Graphics.SurfaceTexture surface, int w, int h) {
			m_TextureView.LayoutParameters = new FrameLayout.LayoutParams(w, h);

			DetectScreenOrientation();

			try {
				m_Camera.StopPreview();
				m_Camera.Release();
				m_Camera.SetPreviewTexture(surface);
				m_Camera.StartPreview();
			} catch (Java.IO.IOException e) {
				System.Console.WriteLine(e.Message);
			}
		}

		public void OnPictureTaken(byte[] data, AndroidCamera camera) {
			try {
				var SaveDir = new Java.IO.File(Environment.GetExternalStoragePublicDirectory(Environment.DirectoryDcim), "Camera");
				if (!SaveDir.Exists()) {
					SaveDir.Mkdir();
				}

				var Files = SaveDir.List();
				int count = 0;
				foreach(var tmp in Files){
					count++;
				}

				var FileName = new Java.IO.File(SaveDir, "DCIM_" + (count + 1) + ".jpg");
				FileOutputStream fos = new FileOutputStream(FileName);

				fos.Write(data);
				fos.Close();

				string[] FilePath = { Environment.GetExternalStoragePublicDirectory(Environment.DirectoryDcim) + "/Camera/" + "DCIM_" + (count + 1) + ".jpg" };
				string[] mimeType = { "image/jpeg" };
				MediaScannerConnection.ScanFile(ApplicationContext, FilePath, mimeType, null);

				Toast.MakeText(ApplicationContext, "保存しました\n" + FileName, ToastLength.Short).Show();
				m_Camera.StartPreview();
			} catch (Exception e) {
				Console.WriteLine(e.Message);
			}
		}

		public void OnAutoFocus(bool success, AndroidCamera camera) {
			if (isTakeEnabled)
				m_Camera.TakePicture(null, null, this);
			isTakeEnabled = false;
		}

		public void DetectScreenOrientation(){
			Display disp = WindowManager.DefaultDisplay;
			var rotation = disp.Rotation;
			if (rotation == SurfaceOrientation.Rotation0 || rotation == SurfaceOrientation.Rotation180) {
				m_Camera.SetDisplayOrientation(90);
			} else {
				m_Camera.SetDisplayOrientation(0);
			}

		}
	}
}


