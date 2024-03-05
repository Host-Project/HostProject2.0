namespace MoenenGames.VoxelRobot {
	using UnityEngine;
	using System.Collections;


	public sealed class PlayerMovement : RobotMovement {




		[SerializeField]
		private bool MouseFacing = false;


		private float prevFTime = -1f;
		private float prevBTime = -1f;
		private float prevLTime = -1f;
		private float prevRTime = -1f;




		protected override void Update () {

			// base
			base.Update();

			// Input
			bool? moveLR;
			bool? moveFB;

			GetPlayerInput(out moveLR, out moveFB);


			// Rot
			if (Input.GetKey(KeyCode.LeftShift) || MouseFacing) {
				RotateToMouse();
			} else if (moveFB != null || moveLR != null) {
				RotateToMovingDirction(moveFB, moveLR);
			}

			// Move
			MoveBasedOnCamera(moveFB, moveLR);

		}



		void GetPlayerInput (out bool? moveLR, out bool? moveFB) {

			moveLR = null;
			moveFB = null;

			if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) {
				if (prevFTime < 0f) {
					prevFTime = Time.time;
				}
				if (prevFTime > prevBTime) {
					moveFB = true;
				}
			} else {
				prevFTime = -1f;
			}

			if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) {
				if (prevBTime < 0f) {
					prevBTime = Time.time;
				}
				if (prevBTime > prevFTime) {
					moveFB = false;
				}
			} else {
				prevBTime = -1f;
			}

			if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) {
				if (prevLTime < 0f) {
					prevLTime = Time.time;
				}
				if (prevLTime > prevRTime) {
					moveLR = false;
				}
			} else {
				prevLTime = -1f;
			}

			if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) {
				if (prevRTime < 0f) {
					prevRTime = Time.time;
				}
				if (prevRTime > prevLTime) {
					moveLR = true;
				}
			} else {
				prevRTime = -1f;
			}

		}




		void RotateToMouse () {
			Vector3 mousePos = GetMouseWorldPosition(transform.position, Vector3.up);
			Rotate(Quaternion.LookRotation(
				mousePos - transform.position
			));
		}



		void RotateToMovingDirction (bool? moveFB, bool? moveLR) {
			Rotate(
				Quaternion.LookRotation(
					new Vector3(
						moveLR == null ? 0f : moveLR.Value ? 1f : -1f,
						0f,
						moveFB == null ? 0f : moveFB.Value ? 1f : -1f
					),
					Vector3.up
				) * Quaternion.Euler(
					0f,
					Camera.main.transform.rotation.eulerAngles.y,
					0f
			));
		}


		void MoveBasedOnCamera (bool? moveFB, bool? moveLR) {
			Move(
				(Camera.main.transform.forward + Camera.main.transform.up) * (moveFB == null ? 0f : moveFB.Value ? 1f : -1f) +
				Camera.main.transform.right * (moveLR == null ? 0f : moveLR.Value ? 1f : -1f)
			);
		}




		private Vector3 GetMouseWorldPosition (Vector3 groundPosition, Vector3 groundNormal) {
			Plane plane = new Plane(groundNormal, groundPosition);
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			float distance;
			if (plane.Raycast(ray, out distance)) {
				return ray.origin + ray.direction * distance;
			}
			return groundPosition;
		}


	}
}