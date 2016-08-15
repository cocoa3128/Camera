using System;

using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Hardware;
using AndroidCamera2 = Android.Hardware.Camera2;
using Android.Opengl;

namespace Camera {
	[Activity(
		Label = "Camera",
		MainLauncher = true,
		Icon = "@mipmap/icon",
		HardwareAccelerated = true,
		ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape,
		ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation
	),Obsolete]
	public class MainActivity : Activity {



		FrameLayout rootView;
		Camera2API mCamera2;

		OrientaionChange mOrientationChange;
		SensorManager mSensorManager;

		GLSurfaceView mGLSurfaceView;

		protected override void OnCreate(Bundle savedInstanceState) {
			base.OnCreate(savedInstanceState);


			Window.AddFlags(WindowManagerFlags.Fullscreen);	// 全画面表示に
			RequestWindowFeature(WindowFeatures.NoTitle);   // タイトルバーをなくす

			rootView = new FrameLayout(ApplicationContext);

			SetContentView(rootView);

			mGLSurfaceView = new GLSurfaceView(ApplicationContext);
			rootView.AddView(mGLSurfaceView);
			mGLSurfaceView.SetEGLContextClientVersion(2);
			mGLSurfaceView.SetRenderer(new GLRenderer(ApplicationContext));

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
			mGLSurfaceView.OnResume();
			rootView.SystemUiVisibility = (StatusBarVisibility)SystemUiFlags.ImmersiveSticky | (StatusBarVisibility)SystemUiFlags.HideNavigation;

			/*mSensorManager.RegisterListener(mOrientationChange,
			                                 mSensorManager.GetDefaultSensor(SensorType.Accelerometer),
			                                 SensorDelay.Ui);

			mSensorManager.RegisterListener(mOrientationChange,
			                                 mSensorManager.GetDefaultSensor(SensorType.MagneticField),
			                                 SensorDelay.Ui);*/

		}

		protected override void OnPause() {
			base.OnPause();
			mGLSurfaceView.OnPause();
			mSensorManager.UnregisterListener(mOrientationChange);
		}

		public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig) {
			mCamera2.OnOrientationChanged();

			Console.WriteLine("Orientation Changed!!!!!");
			base.OnConfigurationChanged(newConfig);
		}

	}
}


