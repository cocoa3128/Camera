using System;
namespace Camera {
	public struct GPUData {

		public int FLOAT_SIZE_BYTES {
			get {
				return 4;
			}
		}

		public string VERTEX_SHADER {
			get{
				return "attribute vec4 position;\n" +
					"attribute vec2 texcoord;\n" +
					"varying vec2 texcoordVarying;\n" +
					"void main() {\n" +
					"    gl_Position = position;\n" +
					"    texcoordVarying = texcoord;\n" +
					"}\n";
			}
		}

		public string FRAGMENT_SHADER{
			get{
				return "#extension GL_OES_EGL_image_external : require\n" +
					"precision mediump float;\n" +
					"varying vec2 texcoordVarying;\n" +
					"uniform samplerExternalOES texture;\n" +
					"void main() {\n" +
					"  gl_FragColor = texture2D(texture, texcoordVarying);\n" +
					"}\n";
			}
		}

		public float[] TEX_COORDS_ROTATION_0 {
			get {
				return new float[]{
					0.0f, 0.0f,
					0.0f, 1.0f,
					1.0f, 0.0f,
					1.0f, 1.0f
				};
			}
		}

		public float[] TEX_COORDS_ROTATION_90 {
			get {
				return new float[]{
					1.0f, 0.0f,
					0.0f, 0.0f,
					1.0f, 1.0f,
					0.0f, 1.0f
				};
			}
		}

		public float[] TEX_COORDS_ROTATION_180 {
			get {
				return new float[]{
					1.0f, 1.0f,
					1.0f, 0.0f,
					0.0f, 1.0f,
					0.0f, 0.0f
				};
			}
		}

		public float[] TEX_COORDS_ROTATION_270 {
			get {
				return new float[]{
					0.0f, 1.0f,
					1.0f, 1.0f,
					0.0f, 0.0f,
					1.0f, 0.0f
				};
			}
		}
		public float[] VERTECES {
			get {
				return new float[]{
					-1.0f, 1.0f, 0.0f,
					-1.0f, -1.0f, 0.0f,
					1.0f, 1.0f, 0.0f,
					1.0f, -1.0f, 0.0f
				};
			}
		}
	}
}

