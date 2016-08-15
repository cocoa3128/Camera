using System;
using System.Collections.Generic;

using Android.App;
using Android.Widget;
using Android.Content;
using Android.Graphics;
using Android.Hardware.Camera2.Params;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Hardware;

using AndroidCamera2 = Android.Hardware.Camera2;
using Matrix = Android.Graphics.Matrix;

namespace Camera {

	public class Camera2API: Activity {
		public enum CameraRotation{
			ROTATION_0,
			ROTATION_90,
			ROTATION_180,
			ROTATION_270,
		}

		struct Vector<Type> {
			
			public Type X { set; get; }
			public Type Y { set; get; }

			public Vector(Type x, Type y){
				X = x;
				Y = y;
			}
		};



		AndroidCamera2.CameraCaptureSession mSession;
		AndroidCamera2.CameraDevice mCamera;
		public AndroidCamera2.CameraManager mCameraManager;
		Context mContext;
		CameraRotation mCameraRotation;
		AndroidCamera2.CaptureRequest.Builder mPreviewBuilder;
		Size mCameraSize;
		TextureView mTextureView;
		Display display;
		Vector<int> DisplaySize;


		public Camera2API(Context context, TextureView texture)  {
			mContext = context;
			mTextureView = texture;
			mCameraManager = (AndroidCamera2.CameraManager)mContext.GetSystemService(Context.CameraService);


			IWindowManager windowManager = mContext.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
			display = windowManager.DefaultDisplay;
			GetDisplaySize();
		}

		public void OpenCamera(AndroidCamera2.LensFacing facing) {
			try {
				foreach (var cameraId in mCameraManager.GetCameraIdList()) {
					
					AndroidCamera2.CameraCharacteristics mCharacteristics = mCameraManager.GetCameraCharacteristics(cameraId);
					if ((int)mCharacteristics.Get(AndroidCamera2.CameraCharacteristics.LensFacing) == GetValueFromKey(facing)) {
						StreamConfigurationMap map = (StreamConfigurationMap)mCharacteristics.Get(AndroidCamera2.CameraCharacteristics.ScalerStreamConfigurationMap);
						mCameraSize = map.GetOutputSizes(Java.Lang.Class.FromType(typeof(SurfaceTexture)))[0];
						HandlerThread thread = new HandlerThread("Open Camera");
						thread.Start();

						Handler BGHandler = new Handler(thread.Looper);
						mCameraManager.OpenCamera(cameraId,
												  new CameraDevice() {
													  OnOpenedAction = (obj) => {
														  mCamera = obj;
														  CreateCaptureSettion();
													  }
												  },
												  BGHandler);

						return;
					}

				}
			}catch(AndroidCamera2.CameraAccessException e){
				Log.Debug("{0}",e.ToString());
			}
		}

		public void CloseCamera(){
			mCamera.Close();
			mCameraManager.UnregisterFromRuntime();
		}

		public void CreateCaptureSettion() {
			if (!mTextureView.IsAvailable)
				return;


			var rotation = 0;
			Vector<float> Scale = new Vector<float>(0, 0);
			DetectCameraRotation();
			switch (mCameraRotation) {
				case CameraRotation.ROTATION_0:
					rotation = 0;
					Scale.X = 1;// (float)mCameraSize.Height / mCameraSize.Width;
					Scale.Y = 1;//(float)mCameraSize.Width / mCameraSize.Height;
					break;
				case CameraRotation.ROTATION_90:
					rotation = 270;
					Scale.X = (float)DisplaySize.Y / DisplaySize.X;
					Scale.Y = (float)DisplaySize.X / DisplaySize.Y;
					break;
				case CameraRotation.ROTATION_180:
					rotation = 180;
					Scale.X = 1;//(float)mCameraSize.Height / mCameraSize.Width;
					Scale.Y = 1;//(float)mCameraSize.Width / mCameraSize.Height;
					break;
				case CameraRotation.ROTATION_270:
					rotation = 90;
					Scale.X = (float)DisplaySize.Y / DisplaySize.X;
					Scale.Y = (float)DisplaySize.X / DisplaySize.Y;
					break;
			}

			Matrix matrix = new Matrix();
			matrix.PostRotate(rotation, DisplaySize.Y / 2, DisplaySize.X / 2);
			matrix.PostScale(Scale.X,
							 Scale.Y,
							 DisplaySize.Y / 2,
							 DisplaySize.X / 2);
			RunOnUiThread(() => {
				mTextureView.SetTransform(matrix);
			});
			Toast.MakeText(mContext, "Width:" + mTextureView.Width +
						   "\nHeight:" + mTextureView.Height,
						   ToastLength.Short).Show();
			SurfaceTexture texture = mTextureView.SurfaceTexture;
			texture.SetDefaultBufferSize(DisplaySize.Y, DisplaySize.X);
			Surface surface = new Surface(texture);


			try {
				mPreviewBuilder = mCamera.CreateCaptureRequest(AndroidCamera2.CameraTemplate.Preview);

			} catch (AndroidCamera2.CameraAccessException e) {
				Log.Debug("{0}", e.ToString());
			}

			mPreviewBuilder.AddTarget(surface);
			var list = new List<Surface>();
			list.Add(surface);
			try {
				mCamera.CreateCaptureSession(new List<Surface>{surface},
											 new CameraCapture() {
												 OnConfiguredAction = (obj) => {
													 mSession = obj;
													 UpdatePreview();
												 }
											 },
											 null);
			} catch (AndroidCamera2.CameraAccessException e) {
				Log.Debug("{0}", e.ToString());
			}
		}

		public void UpdatePreview(){
			mPreviewBuilder.Set(AndroidCamera2.CaptureRequest.ControlAfMode, 4);
			HandlerThread thread = new HandlerThread("Camera Preview");
			thread.Start();
			Handler BGHandler = new Handler(thread.Looper);

			try{
				mSession.SetRepeatingRequest(mPreviewBuilder.Build(), null, BGHandler);
			}catch(AndroidCamera2.CameraAccessException e){
				Log.Debug("{0}", e.ToString());
			}
		}

		public void SetCameraRotation(){

		}



		// 画面のサイズを取得する
		void GetDisplaySize() {
			Point point = new Point(0, 0);
			display.GetRealSize(point);

			if (point.X < point.Y) {
				DisplaySize.X = point.X;
				DisplaySize.Y = point.Y;
			} else {
				DisplaySize.X = point.Y;
				DisplaySize.Y = point.X;
			}
		}

		[Obsolete]
		void DetectCameraRotation(){
			var rotation = display.Rotation;

			switch(rotation){
				case SurfaceOrientation.Rotation0:
					mCameraRotation = CameraRotation.ROTATION_0;
					break;
				case SurfaceOrientation.Rotation90:
					mCameraRotation = CameraRotation.ROTATION_90;
					break;
				case SurfaceOrientation.Rotation180:
					mCameraRotation = CameraRotation.ROTATION_180;
					break;
				case SurfaceOrientation.Rotation270:
					mCameraRotation = CameraRotation.ROTATION_270;
					break;
			}
		}

		public void OnOrientationChanged() {
			Toast.MakeText(mContext, "Rotation Changed", ToastLength.Short).Show();
			CreateCaptureSettion();
			//throw new NotImplementedException();
		}

		int GetValueFromKey(AndroidCamera2.LensFacing request){
			switch(request){
				case AndroidCamera2.LensFacing.Front:
					return 0;
				case AndroidCamera2.LensFacing.Back:
					return 1;
				case AndroidCamera2.LensFacing.External:
					return 2;
				default:
					return 0;
			}
		}
	}
}

