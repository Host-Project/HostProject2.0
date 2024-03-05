namespace MoenenGames.VoxelRobot {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;

	public abstract class Turret : MonoBehaviour, Controllable {



		bool Controllable.Active {
			get {
				return enabled;
			}
			set {
				enabled = value;
				transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
			}
		}



		[SerializeField]
		[Range(0f, 180f)]
		private float Limit = 180f;



		protected float AimRotationY = 0f;
		protected bool CtrlActive = true;



		protected virtual void Start () {
			AimRotationY = transform.localRotation.eulerAngles.y;
		}



		protected virtual void Update () {

			// Rot
			transform.rotation = Quaternion.Euler(0f, AimRotationY, 0f);

			// Clamp
			float localY = Mathf.Repeat(transform.localRotation.eulerAngles.y + 180f, 360f) - 180f;

			if (Mathf.Abs(Mathf.Abs(localY % 360f)) > Limit) {
				transform.localRotation = Quaternion.Euler(0f, (localY > 0f ? Limit : -Limit), 0f);
			}
		}





		public void Rotate (float y) {
			AimRotationY = y;
		}



	}
}