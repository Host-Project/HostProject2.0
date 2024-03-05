namespace MoenenGames.VoxelRobot {
	using UnityEngine;
	using System.Collections;

	public class PlayerTurret : Turret {



		[SerializeField]
		private float RotateSpeed = 720f;



		protected override void Update () {

			Vector3 mousePos = GetMouseWorldPosition(transform.position, Vector3.up);
			if (mousePos != transform.position) {
				Quaternion aimRot = Quaternion.RotateTowards(
					transform.rotation,
					Quaternion.LookRotation(
						mousePos - transform.position,
						Vector3.up
					),
					Time.deltaTime * RotateSpeed
				);
				Rotate(aimRot.eulerAngles.y);
			}


			base.Update();


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