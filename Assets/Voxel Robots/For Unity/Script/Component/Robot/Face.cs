namespace MoenenGames.VoxelRobot {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using System;
#if UNITY_EDITOR
	using UnityEditor;
#endif


	[DisallowMultipleComponent]
	public sealed class Face : MonoBehaviour, Controllable {



		#region --- VAR ---

		// Shot Cut
		bool Controllable.Active {
			get {
				return enabled;
			}
			set {
				enabled = value;
			}
		}


		// Setting
		[Header("Setting")]
		[SerializeField]
		private float BlinkRate = 4f;
		public AnimationCurve BlinkCurve;

		[Header("Component")]
		public Transform EyeL;
		public Transform EyeR;


		// Data
		private FaceType CurrentFace = FaceType.Normal;
		private Vector3 InitEyeScl;
		private Vector3 InitEyeLPos;
		private Vector3 InitEyeRPos;
		private Quaternion InitEyeRot;


		#endregion




		#region --- MSG ---


		void Awake () {
			InitEyeLPos = EyeL.localPosition;
			InitEyeRPos = EyeR.localPosition;
			InitEyeRot = EyeL.localRotation;
			InitEyeScl = EyeL.localScale;
		}



		void Update () {

			Vector3 eyeScl;

			if (CurrentFace == FaceType.Normal) {
				float blink = BlinkCurve.Evaluate(Time.time % BlinkRate);
				eyeScl = new Vector3(
					InitEyeScl.x * (-0.5f * blink + 1.5f),
					InitEyeScl.y * blink,
					1f
				);
			} else {
				eyeScl = InitEyeScl;
			}

			EyeL.localScale = eyeScl;
			EyeR.localScale = eyeScl;

		}


		#endregion




		#region --- API ---



		public void GotoFace (FaceType type) {
			CurrentFace = type;
			switch (type) {
				default:
				case FaceType.Normal:
					EyeL.localPosition = InitEyeLPos;
					EyeR.localPosition = InitEyeRPos;
					EyeL.localRotation = InitEyeRot;
					EyeR.localRotation = InitEyeRot;
					EyeL.localScale = InitEyeScl;
					EyeR.localScale = InitEyeScl;
					break;
				case FaceType.Smile:
					EyeL.localPosition = InitEyeLPos;
					EyeR.localPosition = InitEyeRPos;
					EyeL.localRotation = InitEyeRot;
					EyeR.localRotation = InitEyeRot;
					EyeL.localScale = InitEyeScl;
					EyeR.localScale = InitEyeScl;
					break;
				case FaceType.Awkward:
					EyeL.localPosition = InitEyeLPos;
					EyeR.localPosition = InitEyeRPos;
					EyeL.localRotation = InitEyeRot;
					EyeR.localRotation = InitEyeRot;
					EyeL.localScale = InitEyeScl;
					EyeR.localScale = InitEyeScl;
					break;
			}
		}







		#endregion



	}



#if UNITY_EDITOR


	[CustomEditor(typeof(Face))]
	public class FaceInspector : Editor {




		public override void OnInspectorGUI () {

			base.OnInspectorGUI();

			GUILayout.Space(4f);

			if (GUI.Button(GUILayoutUtility.GetRect(0f, 24f, GUILayout.ExpandWidth(true)), "Set up")) {

				Material eyeMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Material/Robot/Eye.mat");
				Material blushMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Material/Robot/Blush.mat");

				for (int i = 0; i < targets.Length; i++) {

					// Face
					Face f = targets[i] as Face;

					// Eye
					Transform eyeL = f.transform.Find("EyeL");
					if (!eyeL) {
						eyeL = CreateEye(true, f.transform, eyeMat);
					}
					Transform eyeR = f.transform.Find("EyeR");
					if (!eyeR) {
						eyeR = CreateEye(false, f.transform, eyeMat);
					}

					// Blush
					Transform blushL = f.transform.Find("BlushL");
					if (!blushL) {
						blushL = CreateBlush(true, f.transform, blushMat);
					}
					Transform blushR = f.transform.Find("BlushR");
					if (!blushR) {
						blushR = CreateBlush(false, f.transform, blushMat);
					}

					// Done
					f.EyeL = eyeL;
					f.EyeR = eyeR;
					f.BlinkCurve = new AnimationCurve(new Keyframe[] {
					new Keyframe(0f, 0.2f, 0f, 0f),
					new Keyframe(0.14f, 1.1f, 0f, 0f),
					new Keyframe(0.2f, 1f, 0f, 0f),
				});

				}

			}

			GUILayout.Space(4f);

			GUILayout.BeginHorizontal();

			if (GUI.Button(GUILayoutUtility.GetRect(0f, 24f, GUILayout.ExpandWidth(true)), "> <")) {
				for (int i = 0; i < targets.Length; i++) {
					Face f = targets[i] as Face;
					CloseUp(f);
				}
			}

			GUILayout.Space(2f);

			if (GUI.Button(GUILayoutUtility.GetRect(0f, 24f, GUILayout.ExpandWidth(true)), "< >")) {
				for (int i = 0; i < targets.Length; i++) {
					Face f = targets[i] as Face;
					Separate(f);
				}
			}

			GUILayout.EndHorizontal();


			GUILayout.Space(4f);


			GUILayout.BeginHorizontal();

			if (GUI.Button(GUILayoutUtility.GetRect(0f, 24f, GUILayout.ExpandWidth(true)), "♂")) {
				for (int i = 0; i < targets.Length; i++) {
					Face f = targets[i] as Face;
					Transform tf = f.transform.Find("BlushL");
					if (tf) {
						DestroyImmediate(tf.gameObject, true);
					}
					tf = f.transform.Find("BlushR");
					if (tf) {
						DestroyImmediate(tf.gameObject, true);
					}
				}
			}

			GUILayout.Space(2f);

			if (GUI.Button(GUILayoutUtility.GetRect(0f, 24f, GUILayout.ExpandWidth(true)), "♀")) {
				Material blushMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Material/Robot/Blush.mat");
				for (int i = 0; i < targets.Length; i++) {
					Face f = targets[i] as Face;
					Transform tf = f.transform.Find("BlushL");
					if (tf) {
						DestroyImmediate(tf.gameObject, true);
					}
					tf = f.transform.Find("BlushR");
					if (tf) {
						DestroyImmediate(tf.gameObject, true);
					}
					CreateBlush(true, f.transform, blushMat);
					CreateBlush(false, f.transform, blushMat);
				}
			}

			GUILayout.EndHorizontal();


			GUILayout.Space(4f);


		}




		private Transform CreateEye (bool left, Transform root, Material mat) {
			Transform eye = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
			eye.name = left ? "EyeL" : "EyeR";
			eye.SetParent(root);
			eye.localScale = new Vector3(0.1f, 0.2f, 1f);
			eye.localPosition = new Vector3(left ? -0.3f : 0.3f, 0f, 0.01f);
			eye.localRotation = Quaternion.Euler(0f, 180f, 0f);
			if (mat) {
				eye.GetComponent<Renderer>().material = mat;
			}
			DestroyImmediate(eye.GetComponent<Collider>());
			return eye;
		}



		private Transform CreateBlush (bool left, Transform root, Material mat) {
			Transform blush = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
			blush.name = left ? "BlushL" : "BlushR";
			blush.SetParent(root);
			blush.localScale = new Vector3(0.2f, 0.1f, 1f);
			blush.localPosition = new Vector3(left ? -0.35f : 0.35f, -0.17f, 0.05f);
			blush.localRotation = Quaternion.Euler(0f, 180f, 0f);
			if (mat) {
				blush.GetComponent<Renderer>().material = mat;
			}
			DestroyImmediate(blush.GetComponent<Collider>());
			return blush;
		}



		public void CloseUp (Face f) {
			f.EyeL.Translate(-0.05f, 0f, 0f);
			f.EyeR.Translate(0.05f, 0f, 0f);
			Transform b = f.transform.Find("BlushL");
			if (b) {
				b.Translate(-0.05f, 0f, 0f);
			}
			b = f.transform.Find("BlushR");
			if (b) {
				b.Translate(0.05f, 0f, 0f);
			}
		}


		public void Separate (Face f) {
			f.EyeL.Translate(0.05f, 0f, 0f);
			f.EyeR.Translate(-0.05f, 0f, 0f);
			Transform b = f.transform.Find("BlushL");
			if (b) {
				b.Translate(0.05f, 0f, 0f);
			}
			b = f.transform.Find("BlushR");
			if (b) {
				b.Translate(-0.05f, 0f, 0f);
			}
		}


	}


#endif
}