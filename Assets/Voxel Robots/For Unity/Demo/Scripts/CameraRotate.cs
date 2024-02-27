namespace MoenenGames.LowpolyParticle {

	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;

	public class CameraRotate : MonoBehaviour {





		[Header("Rotate")]
		[SerializeField]
		private float RotateSpeed = 4f;
		[SerializeField]
		private Vector2 RotateLimitAngelY = new Vector2(-90f, 90f);

		[Header("Zoom")]
		[SerializeField]
		private float ZoomSpeed = 4f;
		[SerializeField]
		private Vector2 ZoomLimitDistance = new Vector2(-39f, -5f);


		// Data
		private Transform CameraTF;



		private void Awake () {
			CameraTF = transform.GetChild(0);
		}



		private void Update () {

			CameraRotateUpdate();

			ZoomUpdate();

		}


		private void CameraRotateUpdate () {

			// Check
			if (Input.GetMouseButton(1)) {

				Vector3 mouseDel = new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), 0) * RotateSpeed;

				float angelX = transform.rotation.eulerAngles.y + mouseDel.x;
				float angelY = transform.rotation.eulerAngles.x - mouseDel.y;

				angelY = ClampAngel(angelY, RotateLimitAngelY.x, RotateLimitAngelY.y);

				transform.rotation = Quaternion.Euler(angelY, angelX, 0f);

			}

		}



		private void ZoomUpdate () {
			float delta = Input.GetAxis("Mouse ScrollWheel");
			if (delta != 0f) {
				Vector3 pos = CameraTF.localPosition;
				pos.z = Mathf.Clamp(pos.z + delta * ZoomSpeed, ZoomLimitDistance.x, ZoomLimitDistance.y);
				CameraTF.localPosition = pos;
			}
		}






		#region --- UTL ---




		private float ClampAngel (float angel, float min, float max) {

			if (max - min == 360f || max - min == 720f) {
				return Mathf.Repeat(angel, 360f);
			}

			float a0 = Mathf.Repeat(angel + 360f, 720f) - 360f;
			float a1 = Mathf.Repeat(angel, 720f) - 360f;
			min = Mathf.Repeat(min + 360f, 720f) - 360f;
			max = Mathf.Repeat(max + 360f, 720f) - 360f;

			if ((a0 < min || a0 > max) && (a1 < min || a1 > max)) {
				return Mathf.Min(
						Mathf.Abs(a0 - min),
						Mathf.Abs(a1 - min)) <
					Mathf.Min(
						Mathf.Abs(a0 - max),
						Mathf.Abs(a1 - max)
					) ? min : max;
			} else {
				return a0;
			}

		}




		#endregion








	}
}