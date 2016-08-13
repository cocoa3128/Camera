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
	public class MainActivity : Activity,ILocationListener, TextureView.ISurfaceTextureListener, AndroidCamera.IPictureCallback, AndroidCamera.IAutoFocusCallback,AndroidCamera.IAutoFocusMoveCallback {

		const uint ButtonWidth = 200;
		const uint ButtonHeight = 150;

		public AndroidCamera m_Camera;
		TextureView m_TextureView;
		bool isTakeEnabled = false;
		bool isGPSEnabled = false;
		Display disp;
		int DisplayWidth = 1080;
		int DisplayHeight = 1920;
		Location m_Location;
		LocationManager m_LocationManager;
		string Latitude = "";
		string Longitude = "";
		int NowOriantation = 0;

		float StartX = 0;
		float StartY = 0;

		protected override void OnCreate(Bundle savedInstanceState) {
			base.OnCreate(savedInstanceState);

			Window.AddFlags(WindowManagerFlags.Fullscreen);	// 全画面表示に
			RequestWindowFeature(WindowFeatures.NoTitle);   // タイトルバーをなくす


			disp = WindowManager.DefaultDisplay;
			GetDisplaySize();


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
			button.SetWidth(200);
			button.SetHeight(150);


			if (!isLandScapeMode()) {
				button.SetX((DisplayWidth / 2) -  100);
				button.SetY(DisplayHeight - 300);
			}else{
				button.SetX((DisplayHeight / 2) - 100);
				button.SetY(DisplayWidth - 150);
			}

			button.Touch += (sender, e) => {
				if (e.Event.Action == MotionEventActions.Down){
					isTakeEnabled = false;
					m_Camera.AutoFocus(this);
				}

				if((!isTakeEnabled) && (e.Event.Action == MotionEventActions.Up)) {
					// 撮影中ではない & ボタンから指が離れた
					if ((e.Event.RawX > button.GetX()) &&
					    (e.Event.RawX < (button.GetX() + ButtonWidth)) &&
					   (e.Event.RawY > button.GetY()) &&
					    (e.Event.RawY < (button.GetY() + ButtonHeight))) {
						// 指が離れた時にボタン上に指があった
						isTakeEnabled = true;
						m_Camera.AutoFocus(this);
					}

					Toast.MakeText(ApplicationContext,
								   "ButtonX:" + button.GetX() +
								   "\nButtonY:" + button.GetY() +
					               "\nGetX:" + e.Event.RawX +
					               "\nGetY:" + e.Event.RawY,
								   ToastLength.Short).Show();
				}
			};

			rootView.AddView(button);

			m_LocationManager = (LocationManager)GetSystemService(Context.LocationService);

		}

		protected override void OnResume() {
			NowOriantation = DetectScreenOrientation();

			m_Camera = AndroidCamera.Open();
			m_Camera.SetAutoFocusMoveCallback(this);

			// 写真のサイズを画面サイズの4倍(縦横2倍)に設定
			var param = m_Camera.GetParameters();
			param.SetPictureSize(DisplayHeight * 2, DisplayWidth * 2);
			m_Camera.SetParameters(param);

			m_LocationManager.RequestLocationUpdates(LocationManager.GpsProvider,
													1,
													0,
													 this);
			base.OnResume();

		}

		protected override void OnPause() {
			m_LocationManager.RemoveUpdates(this);

			m_Camera.StopPreview();
			m_Camera.Release();
			base.OnPause();
		}

		#region GPSリスナー
		public void OnProviderEnabled(string provider) {

		}

		public void OnProviderDisabled(string provider) {

		}

		public void OnStatusChanged(string provider, Availability available, Bundle bundle){
			switch(available){
				case Availability.Available:
					break;
				case Availability.OutOfService:
					break;
				case Availability.TemporarilyUnavailable:
					break;
			}
		}

		public void OnLocationChanged(Location location){
			m_Location = location;
			Console.WriteLine("Latitude" + location.Latitude);
			Console.WriteLine("Longitude" + location.Longitude);
			Console.WriteLine("Accuracy" + location.Accuracy);
			Console.WriteLine("Altitude" + location.Altitude);
			Console.WriteLine("Time" + location.Time);
			Console.WriteLine("Speed" + location.Speed);
			Console.WriteLine("Bering" + location.Bearing);


			int degree = (int)(location.Latitude);
			int minute = (int)((location.Latitude - degree) * 60);
			int second = (int)((((location.Latitude - degree) * 60) - minute) * 60000);
			Latitude = degree + "/," + minute + "/1," + second + "/1000";


			degree = (int)(location.Latitude);
			minute = (int)((location.Latitude - degree) * 60);
			second = (int)((((location.Latitude - degree) * 60) - minute) * 60000);
			Longitude = degree + "/," + minute + "/1," + second + "/1000";
		}

		#endregion

		#region SurfaceViewのリスナー
		public void OnSurfaceTextureAvailable(Android.Graphics.SurfaceTexture surface, int w, int h) {

			bool landscape = isLandScapeMode();
			if(!landscape)
				m_TextureView.LayoutParameters = new FrameLayout.LayoutParams(DisplayWidth, DisplayHeight);
			else
				m_TextureView.LayoutParameters = new FrameLayout.LayoutParams(DisplayHeight, DisplayWidth);

			SetScreenOrientation();

			try {
				m_Camera.SetPreviewTexture(surface);
				m_Camera.StartPreview();
			} catch (Java.IO.IOException e) {
				System.Console.WriteLine(e.Message);
			}
		}

		public bool OnSurfaceTextureDestroyed(Android.Graphics.SurfaceTexture surface) {
			return true;
		}

		// SurfaceViewが更新された時に呼び出される
		public void OnSurfaceTextureUpdated(Android.Graphics.SurfaceTexture surface) {
			if (NowOriantation == DetectScreenOrientation())
				return;

			NowOriantation = DetectScreenOrientation();
			SetScreenOrientation();
		}

		public void OnSurfaceTextureSizeChanged(Android.Graphics.SurfaceTexture surface, int w, int h) {
			bool landscape = isLandScapeMode();
			if (!landscape)
				m_TextureView.LayoutParameters = new FrameLayout.LayoutParams(DisplayWidth, DisplayHeight);
			else
				m_TextureView.LayoutParameters = new FrameLayout.LayoutParams(DisplayHeight, DisplayWidth);

			SetScreenOrientation();

			try {
				m_Camera.StopPreview();
				m_Camera.SetPreviewTexture(surface);
				m_Camera.StartPreview();
			} catch (Java.IO.IOException e) {
				System.Console.WriteLine(e.Message);
			}
		}
		#endregion

		#region カメラのリスナー
		// シャッターを切った時のコールバック
		public void OnPictureTaken(byte[] data, AndroidCamera camera) {
			try {
				var SaveDir = new Java.IO.File(Environment.GetExternalStoragePublicDirectory(Environment.DirectoryDcim), "Camera");
				if (!SaveDir.Exists()) {
					SaveDir.Mkdir();
				}


				// 非同期で画像の回転・保存・アルバムへの登録
				Task.Run(async () => {

					// 保存ディレクトリに入ってるファイル数をカウント
					var Files = SaveDir.List();
					int count = 0;
					foreach (var tmp in Files) {
						count++;
					}

					Matrix matrix = new Matrix();   // 回転用の行列
					matrix.SetRotate(90 - DetectScreenOrientation());
					Bitmap original = BitmapFactory.DecodeByteArray(data, 0, data.Length);
					Bitmap rotated = Bitmap.CreateBitmap(original, 0, 0, original.Width, original.Height, matrix, true);

					var FileName = new Java.IO.File(SaveDir, "DCIM_" + (count + 1) + ".jpg");


					// ファイルをストレージに保存
					FileStream stream = new FileStream(FileName.ToString(), FileMode.CreateNew);
					await rotated.CompressAsync(Bitmap.CompressFormat.Jpeg, 90, stream);
					stream.Close();

					Android.Media.ExifInterface Exif = new ExifInterface(FileName.ToString());
					Exif.SetAttribute(ExifInterface.TagGpsLatitude, Latitude);
					Exif.SetAttribute(ExifInterface.TagGpsLongitude, Longitude);
					Exif.SetAttribute(ExifInterface.TagGpsLatitudeRef, "N");
					Exif.SetAttribute(ExifInterface.TagGpsLongitudeRef, "E");
					Exif.SaveAttributes();


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

		public void OnAutoFocusMoving(bool success, AndroidCamera camea){
			
		}
		#endregion

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


