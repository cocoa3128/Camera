using System;
using System.IO;
using System.Threading.Tasks;

using Android.App;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Views;
using Android.Widget;

using AndroidCamera = Android.Hardware.Camera;
using Console = System.Console;
using Environment = Android.OS.Environment;
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
		bool isTakeEnabled = false;
		Display disp;
		int DisplayWidth = 1080;
		int DisplayHeight = 1920;

		float StartX = 0;
		float StartY = 0;

		protected override void OnCreate(Bundle savedInstanceState) {
			base.OnCreate(savedInstanceState);
			disp = WindowManager.DefaultDisplay;

			GetDisplaySize();

			Window.AddFlags(WindowManagerFlags.Fullscreen);	// 全画面表示に
			RequestWindowFeature(WindowFeatures.NoTitle);	// タイトルバーをなくす

			FrameLayout rootView = new FrameLayout(ApplicationContext);
			SetContentView(rootView);

			m_TextureView = new TextureView(ApplicationContext);
			m_TextureView.SurfaceTextureListener = this;

			m_TextureView.Touch += (sender, e) => {
				if(e.Event.Action == MotionEventActions.Down){
					StartX = e.Event.GetX();
					StartY = e.Event.GetY();
				}

				if (e.Event.Action == MotionEventActions.Move) {

					var param = m_Camera.GetParameters();
					var Zoom = param.Zoom;

					if(e.Event.GetY() > (StartY + 50)){
						Zoom--;
						StartY = e.Event.GetY();
					}else if (e.Event.GetY() < (StartY - 50)) {
						Zoom++;
						StartY = e.Event.GetY();
					}


					if (Zoom > param.MaxZoom)
						Zoom = param.MaxZoom;
					if (Zoom < 1)
						Zoom = 1;

					param.Zoom = Zoom;
					m_Camera.SetParameters(param);

					Console.WriteLine("StartY:" + StartY +
									  "\nNowY:" + e.Event.GetY() +
									  "\nZoom" + Zoom);
				}
			};

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

				if((!isTakeEnabled) && (e.Event.Action == MotionEventActions.Up)) {
					// 撮影中ではない & ボタンから指が離れた
					if ((e.Event.GetX() > button.GetX()) &&
					    (e.Event.GetX() < (button.GetX() + button.Width)) &&
					   (e.Event.GetY() > button.GetY()) &&
					    (e.Event.GetY() < (button.GetY() + button.Height))) {
						// 指が離れた時にボタン上に指があった
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

		// SurfaceViewが更新された時に呼び出される
		public void OnSurfaceTextureUpdated(Android.Graphics.SurfaceTexture surface) {
			SetScreenOrientation();

			// 写真のサイズを画面サイズの4倍(縦横2倍)に設定
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

		// シャッターを切った時のコールバック
		public void OnPictureTaken(byte[] data, AndroidCamera camera) {
			try {
				var SaveDir = new Java.IO.File(Environment.GetExternalStoragePublicDirectory(Environment.DirectoryDcim), "Camera");
				if (!SaveDir.Exists()) {
					SaveDir.Mkdir();
				}

				// 保存ディレクトリに入ってるファイル数をカウント
				var Files = SaveDir.List();
				int count = 0;
				foreach(var tmp in Files){
					count++;
				}

				Matrix matrix = new Matrix();	// 回転用の行列
				matrix.SetRotate(90 - DetectScreenOrientation());
				Bitmap original = BitmapFactory.DecodeByteArray(data, 0, data.Length);
				Bitmap rotated = Bitmap.CreateBitmap(original, 0, 0, original.Width, original.Height, matrix, true);

				var FileName = new Java.IO.File(SaveDir, "DCIM_" + (count + 1) + ".jpg");

				// 非同期で画面の回転処理とアルバムへの登録を行う
				Task.Run(async () => {

					// ファイルをストレージに保存
					FileStream stream = new FileStream(FileName.ToString(), FileMode.CreateNew);
					await rotated.CompressAsync(Bitmap.CompressFormat.Jpeg, 90, stream);
					stream.Close();

					// 保存したファイルをアルバムに登録
					string[] FilePath = { Environment.GetExternalStoragePublicDirectory(Environment.DirectoryDcim) + "/Camera/" + "DCIM_" + (count + 1) + ".jpg" };
					string[] mimeType = { "image/jpeg" };
					MediaScannerConnection.ScanFile(ApplicationContext, FilePath, mimeType, null);
					RunOnUiThread(() => {
						Toast.MakeText(ApplicationContext, "保存しました\n" + FileName, ToastLength.Short).Show();
					});
					original.Recycle();
					rotated.Recycle();

					isTakeEnabled = false;
				});

				m_Camera.StartPreview();
			} catch (Exception e) {
				Console.WriteLine(e.Message);
			}
		}

		// AutoFocusのコールバック
		public void OnAutoFocus(bool success, AndroidCamera camera) {
			if (isTakeEnabled)
				m_Camera.TakePicture(null, null, this);
		}

		// 画面の向きに合わせてSurfaceViewの向きを変更
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

		// 角度単位で画面の向きを検出
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

		// 横向きかどうかを確認する
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


		// 画面のサイズを取得する
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


