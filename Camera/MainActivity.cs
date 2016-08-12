using Android.App;
using Android.Widget;
using Android.OS;

using Android.Hardware;
using Android.Hardware.Camera2;
using Android.Hardware.Camera2.Params;
using Android.Views;
using Android.Media;
using Android.Graphics;

using Android.Util;

using System;
using System.IO;
using Java.IO;

using AndroidCamera = Android.Hardware.Camera;
using Environment = Android.OS.Environment;
using Console = System.Console;
using Matrix = Android.Graphics.Matrix;

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
		Display disp;
		int DisplayWidth = 1080;
		int DisplayHeight = 1920;

		protected override void OnCreate(Bundle savedInstanceState) {
			base.OnCreate(savedInstanceState);
			disp = WindowManager.DefaultDisplay;

			GetDisplaySize();

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

			bool landscape = isLandScapeMode();
			if(!landscape)
				m_TextureView.LayoutParameters = new FrameLayout.LayoutParams(DisplayWidth * 2, DisplayHeight * 2);
			else
				m_TextureView.LayoutParameters = new FrameLayout.LayoutParams(DisplayHeight * 2, DisplayWidth * 2);

			SetScreenOrientation();

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
			SetScreenOrientation();

			var param = m_Camera.GetParameters();

			param.SetPictureSize(DisplayHeight * 2 , DisplayWidth * 2);
					
			m_Camera.SetParameters(param);

		}

		public void OnSurfaceTextureSizeChanged(Android.Graphics.SurfaceTexture surface, int w, int h) {
			bool landscape = isLandScapeMode();
			if (!landscape)
				m_TextureView.LayoutParameters = new FrameLayout.LayoutParams(DisplayWidth * 2, DisplayHeight * 2);
			else
				m_TextureView.LayoutParameters = new FrameLayout.LayoutParams(DisplayHeight * 2, DisplayWidth * 2);

			SetScreenOrientation();

			try {
				m_Camera.StopPreview();
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

				Matrix matrix = new Matrix();
				matrix.SetRotate(90 - DetectScreenOrientation());
				Bitmap original = BitmapFactory.DecodeByteArray(data, 0, data.Length);
				Bitmap rotated = Bitmap.CreateBitmap(original, 0, 0, original.Width, original.Height, matrix, true);

				var FileName = new Java.IO.File(SaveDir, "DCIM_" + (count + 1) + ".jpg");
				//FileOutputStream fos = new FileOutputStream(FileName);
				System.IO.FileStream stream = new FileStream(FileName.ToString(), FileMode.CreateNew);
				//fos.Write(data);
				rotated.Compress(Bitmap.CompressFormat.Jpeg, 90, stream);
				//fos.Close();
				stream.Close();

				string[] FilePath = { Environment.GetExternalStoragePublicDirectory(Environment.DirectoryDcim) + "/Camera/" + "DCIM_" + (count + 1) + ".jpg" };
				string[] mimeType = { "image/jpeg" };
				MediaScannerConnection.ScanFile(ApplicationContext, FilePath, mimeType, null);

				Toast.MakeText(ApplicationContext, "保存しました\n" + FileName, ToastLength.Short).Show();
				m_Camera.StartPreview();

				original.Recycle();
				rotated.Recycle();
			} catch (Exception e) {
				Console.WriteLine(e.Message);
			}
		}

		public void OnAutoFocus(bool success, AndroidCamera camera) {
			if (isTakeEnabled)
				m_Camera.TakePicture(null, null, this);
			isTakeEnabled = false;
		}

		public bool SetScreenOrientation(){
			var orientation = DetectScreenOrientation();
			bool ReturnValue = false;
			if ((orientation == 0) || (orientation == 180)) {
				m_Camera.SetDisplayOrientation(90);
				ReturnValue = false;
			} else if(orientation == 90) {
				m_Camera.SetDisplayOrientation(0);
				ReturnValue = true;
			}else if(orientation == 270){
				m_Camera.SetDisplayOrientation(180);
				ReturnValue = true;
			}

			return ReturnValue;
		}

		public int DetectScreenOrientation(){
			var rotation = disp.Rotation;
			int ReturnValue = 0;

			switch (rotation) {
				case SurfaceOrientation.Rotation0:
					ReturnValue = 0;
					break;
				case SurfaceOrientation.Rotation90:
					ReturnValue = 90;
					break;
				case SurfaceOrientation.Rotation180:
					ReturnValue = 180;
					break;
				case SurfaceOrientation.Rotation270:
					ReturnValue = 270;
					break;
			}
			return ReturnValue;
		}

		public bool isLandScapeMode(){
			var orientation = DetectScreenOrientation();
			bool ReturnValue = false;

			if ((orientation == 0) || (orientation == 180)) {
				ReturnValue = false;
			} else{
				ReturnValue = true;
			}

			return ReturnValue;

		}


		public void GetDisplaySize(){
			Point point = new Point(0, 0);
			disp.GetRealSize(point);

			if (point.X < point.Y) {
				DisplayWidth = point.X;
				DisplayHeight = point.Y;
			}else{
				DisplayWidth = point.Y;
				DisplayHeight = point.X;
			}
		}
	}
}


