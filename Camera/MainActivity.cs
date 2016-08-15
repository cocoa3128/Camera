using System;

using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Hardware;
using AndroidCamera2 = Android.Hardware.Camera2;

namespace Camera {
	[Activity(
		Label = "Camera",
		MainLauncher = true,
		Icon = "@mipmap/icon",
		HardwareAccelerated = true,
		ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape,
		ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation
	),Obsolete]
	public class MainActivity : Activity,TextureView.ISurfaceTextureListener {



		TextureView mTextureView;
		FrameLayout rootView;
		Camera2API mCamera2;

		OrientaionChange mOrientationChange;
		SensorManager mSensorManager;


		protected override void OnCreate(Bundle savedInstanceState) {
			base.OnCreate(savedInstanceState);


			Window.AddFlags(WindowManagerFlags.Fullscreen);	// 全画面表示に
			RequestWindowFeature(WindowFeatures.NoTitle);   // タイトルバーをなくす

			rootView = new FrameLayout(ApplicationContext);

			SetContentView(rootView);


			mTextureView = new TextureView(ApplicationContext);
			mTextureView.SurfaceTextureListener = this;
	
			rootView.AddView(mTextureView);

			mCamera2 = new Camera2API(ApplicationContext, mTextureView);

			mOrientationChange = new OrientaionChange(ApplicationContext);

			mSensorManager = (SensorManager)GetSystemService(SensorService);


			/* 度分秒に変換するやつ
			degree = (int)(location.Latitude);
			minute = (int)((location.Latitude - degree) * 60);
			second = (int)((((location.Latitude - degree) * 60) - minute) * 60000);
			Longitude = degree + "/," + minute + "/1," + second + "/1000";
			*/

		}

		protected override void OnResume() {
			base.OnResume();
			rootView.SystemUiVisibility = (StatusBarVisibility)SystemUiFlags.ImmersiveSticky | (StatusBarVisibility)SystemUiFlags.HideNavigation;

			mSensorManager.RegisterListener(mOrientationChange,
			                                 mSensorManager.GetDefaultSensor(SensorType.Accelerometer),
			                                 SensorDelay.Ui);

			mSensorManager.RegisterListener(mOrientationChange,
			                                 mSensorManager.GetDefaultSensor(SensorType.MagneticField),
			                                 SensorDelay.Ui);


		}

		protected override void OnPause() {
			mSensorManager.UnregisterListener(mOrientationChange);
			base.OnPause();
		}

		#region SurfaceViewのリスナー
		public void OnSurfaceTextureAvailable(Android.Graphics.SurfaceTexture surface, int w, int h) {
			mCamera2.OpenCamera(AndroidCamera2.LensFacing.Back);
		}

		public bool OnSurfaceTextureDestroyed(Android.Graphics.SurfaceTexture surface) {
			mCamera2.CloseCamera();
			return false;
		}

		public void OnSurfaceTextureUpdated(Android.Graphics.SurfaceTexture surface) {
		}

		public void OnSurfaceTextureSizeChanged(Android.Graphics.SurfaceTexture surface, int w, int h) {
		}
		#endregion

		public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig) {
			mCamera2.OnOrientationChanged();

			Console.WriteLine("Orientation Changed!!!!!");
			base.OnConfigurationChanged(newConfig);
		}

	}
}


