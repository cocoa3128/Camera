
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Opengl;
using Javax.Microedition.Khronos.Egl;
using Javax.Microedition.Khronos.Opengles;
using Java.Nio;

namespace Camera {
	public class GLRenderer : View, GLSurfaceView.IRenderer {

		GPUData mGPUData { get; }
		int mTextureID;
		Camera2API mCamera;
		FloatBuffer mTexCoordBuffer;
		FloatBuffer mVertexBuffer;
		int mProgram;
		int mPositionHandle;
		int mTexCoordHandle;
		int mTextureHandle;
		bool mConfigured = false;

		Context mContext;

		public GLRenderer(Context context) :
			base(context) {
			mContext = context;
			
			Initialize();
		}

		public GLRenderer(Context context, IAttributeSet attrs) :
			base(context, attrs) {
			Initialize();
		}

		public GLRenderer(Context context, IAttributeSet attrs, int defStyle) :
			base(context, attrs, defStyle) {
			Initialize();
		}

		public void OnDrawFrame(IGL10 gl) {
			GLES20.GlClearColor(0.5f, 0.5f, 1.0f, 1.0f);
			GLES20.GlClear(GLES20.GlDepthBufferBit | GLES20.GlColorBufferBit);

			if (!mConfigured) {
				if (mConfigured = mCamera.mInitialized) {
					SetConfiration();
				} else {
					return;
				}
			}

			mCamera.UpdateTexture();

			GLES20.GlUseProgram(mProgram);

			GLES20.GlVertexAttribPointer(mTexCoordHandle, 2, GLES20.GlFloat, false, 0, mTexCoordBuffer);
			GLES20.GlVertexAttribPointer(mPositionHandle, 3, GLES20.GlFloat, false, 0, mVertexBuffer);
			CheckGLError("glVertexAttribPointer");

			GLES20.GlUniform1i(mTextureHandle, 0);
			CheckGLError("glUniform1i");

			GLES20.GlActiveTexture(GLES20.GlTexture0);
			GLES20.GlBindTexture(GLES11Ext.GlTextureExternalOes, mTextureID);
			CheckGLError("glBindTexture");

			GLES20.GlDrawArrays(GLES20.GlTriangleStrip, 0, 4);

			GLES20.GlUseProgram(0);
			GLES20.GlBindTexture(GLES11Ext.GlTextureExternalOes, 0);
		}

		public void OnSurfaceChanged(IGL10 gl, int width, int height) {
			mConfigured = false;
		}

		public void OnSurfaceCreated(IGL10 unused, Javax.Microedition.Khronos.Egl.EGLConfig config) {
			int[] textures = new int[1];

			GLES20.GlGenTextures(1, textures, 0);
			mTextureID = textures[0];

			GLES20.GlBindTexture(GLES11Ext.GlTextureExternalOes, mTextureID);
			GLES20.GlTexParameterf(GLES11Ext.GlTextureExternalOes, GL10.GlTextureMinFilter, GL10.GlLinear);
			GLES20.GlTexParameterf(GLES11Ext.GlTextureExternalOes, GL10.GlTextureMagFilter, GL10.GlLinear);
			GLES20.GlTexParameteri(GLES11Ext.GlTextureExternalOes, GL10.GlTextureWrapS, GL10.GlClampToEdge);
			GLES20.GlTexParameteri(GLES11Ext.GlTextureExternalOes, GL10.GlTextureWrapT, GL10.GlClampToEdge);

			mCamera = new Camera2API(mContext, mTextureID);
			mCamera.OpenCamera(Android.Hardware.Camera2.LensFacing.Back);

			mTexCoordBuffer = ByteBuffer.AllocateDirect(mGPUData.TEX_COORDS_ROTATION_90.Length * mGPUData.FLOAT_SIZE_BYTES)
			                            .Order(ByteOrder.NativeOrder())
			                            .AsFloatBuffer();

			mVertexBuffer = ByteBuffer.AllocateDirect(mGPUData.VERTECES.Length * mGPUData.FLOAT_SIZE_BYTES)
										   .Order(ByteOrder.NativeOrder())
										   .AsFloatBuffer();
			mVertexBuffer.Put(mGPUData.VERTECES).Position(0);

			mProgram = CreateProgram(mGPUData.VERTEX_SHADER, mGPUData.FRAGMENT_SHADER);

			mPositionHandle = GLES20.GlGetAttribLocation(mProgram, "position");
			GLES20.GlEnableVertexAttribArray(mPositionHandle);
			mTexCoordHandle = GLES20.GlGetAttribLocation(mProgram, "texcoord");
			GLES20.GlEnableVertexAttribArray(mTexCoordHandle);
			CheckGLError("glGetAttributeLocation");

			mTextureHandle = GLES20.GlGetUniformLocation(mProgram, "texture");
			CheckGLError("glGetUniformLocation");
		}

		int loadShader(int shaderType, String source){
			int shader = GLES20.GlCreateShader(shaderType);

			if(shader != 0){
				GLES20.GlShaderSource(shader, source);
				GLES20.GlCompileShader(shader);

				int[] compileid = new int[1];

				GLES20.GlGetShaderiv(shader, GLES20.GlCompileStatus, compileid, 0);

				if(compileid[0] == 0){
					GLES20.GlDeleteShader(shader);
					shader = 0;
				}
			}

			return shader;
		}

		int CreateProgram(string vertexSource, string fragmentSource){
			
			int vertexShader = loadShader(GLES20.GlVertexShader, vertexSource);
			if (vertexShader == 0)
				return 0;

			int pixelShader = loadShader(GLES20.GlFragmentShader, fragmentSource);
			if (pixelShader == 0)
				return 0;

			int program = GLES20.GlCreateProgram();
			if (program == 0)
				return 0;

			GLES20.GlAttachShader(program, vertexShader);
			GLES20.GlAttachShader(program, pixelShader);
			GLES20.GlLinkProgram(program);

			int[] linkStatus = new int[1];
			GLES20.GlGetProgramiv(program, GLES20.GlLinkStatus, linkStatus, 0);
			if(linkStatus[0] != GLES20.GlTrue){
				GLES20.GlDeleteProgram(program);
				program = 0;
			}

			return program;
		}

		void CheckGLError(String op){
			int error = GLES20.GlGetError();

			if(error != GLES20.GlNoError){
				throw new AndroidRuntimeException(op + ":glError" + error);
			}
		}

		void SetConfiration(){
			mTexCoordBuffer.Position(0);

			Point displaySize = mCamera.GetDisplaySize();

			GLES20.GlViewport(0, 0, displaySize.X, displaySize.Y);
			Log.Debug("ofjsp", "Disp.X:{0}  Disp.Y:{1}", displaySize.X, displaySize.Y);
		}

		void Initialize() {
		}
	}
}

