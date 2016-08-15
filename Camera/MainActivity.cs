using System;

using Android.App;
using Android.Content;
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
	)]
	[Obsolete]
	public class MainActivity : Activity,TextureView.ISurfaceTextureListener {



		TextureView m_TextureView;
		FrameLayout rootView;
		Camera2API m_Camera2;

		OrientaionChange m_OrientationChange;
		SensorManager m_SensorManager;


		protected override void OnCreate(Bundle savedInstanceState) {
			base.OnCreate(savedInstanceState);


			Window.AddFlags(WindowManagerFlags.Fullscreen);	// 全画面表示に
			RequestWindowFeature(WindowFeatures.NoTitle);   // タイトルバーをなくす

			rootView = new FrameLayout(ApplicationContext);

			SetContentView(rootView);


			m_TextureView = new TextureView(ApplicationContext);
			m_TextureView.SurfaceTextureListener = this;
	
			rootView.AddView(m_TextureView);

			m_Camera2 = new Camera2API(ApplicationContext, m_TextureView);

			m_OrientationChange = new OrientaionChange(ApplicationContext);

			m_SensorManager = (SensorManager)GetSystemService(Context.SensorService);


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

			m_SensorManager.RegisterListener(m_OrientationChange,
			                                 m_SensorManager.GetDefaultSensor(SensorType.Accelerometer),
			                                 SensorDelay.Ui);

			m_SensorManager.RegisterListener(m_OrientationChange,
											 m_SensorManager.GetDefaultSensor(SensorType.MagneticField),
			                                 SensorDelay.Ui);


			Toast.MakeText(ApplicationContext, "第二段階", ToastLength.Short).Show();

		}

		protected override void OnPause() {
			m_SensorManager.UnregisterListener(m_OrientationChange);
			base.OnPause();
		}

		#region SurfaceViewのリスナー
		public void OnSurfaceTextureAvailable(Android.Graphics.SurfaceTexture surface, int w, int h) {
			m_Camera2.OpenCamera(AndroidCamera2.LensFacing.Front);
		}

		public bool OnSurfaceTextureDestroyed(Android.Graphics.SurfaceTexture surface) {
			m_Camera2.CloseCamera();
			return false;
		}

		public void OnSurfaceTextureUpdated(Android.Graphics.SurfaceTexture surface) {
		}

		public void OnSurfaceTextureSizeChanged(Android.Graphics.SurfaceTexture surface, int w, int h) {
		}
		#endregion

		public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig) {
			m_Camera2.OnOrientationChanged();

			Console.WriteLine("Orientation Changed!!!!!");
			base.OnConfigurationChanged(newConfig);
		}

	}
}


