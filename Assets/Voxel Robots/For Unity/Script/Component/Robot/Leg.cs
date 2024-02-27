namespace MoenenGames.VoxelRobot {
	using UnityEngine;
	using System.Collections;
	using UnityEngine.Events;
	using System.Collections.Generic;


	[DisallowMultipleComponent]
	public class Leg : MonoBehaviour {


		// Shot Cut
		public bool Moving {
			get {
				return MovingTime > Time.time - CONFLICT_MOVING_TIME;
			}
		}

		public float Distance {
			get {
				Vector3 pos = transform.localPosition;
				pos.y = InitLocalPos.y;
				return Vector3.Distance(pos, InitLocalPos);
			}
		}

		public float SlideDistance {
			get {
				return StepDistance * 2f;
			}
		}

		public float StepLength {
			get {
				return StepDistance * 0.75f;
			}
		}

		public bool Sliping {
			get; set;
		}



		// Serialize
		[Header("Setting")]
		[SerializeField]
		private float StepDistance = 0.3f;

		[Header("Component")]
		[SerializeField]
		private Leg ConflictLegA = null;
		[SerializeField]
		private Leg ConflictLegB = null;



		// Data
		private float MovingTime = float.MinValue;
		private Vector3 InitLocalPos = Vector3.zero;
		private Quaternion InitLocalRot = Quaternion.identity;
		private Vector3 PrevPos;
		private Quaternion PrevRot;
		private Vector3 AimPos;
		private CharacterController Chr = null;


		// Setting
		private const float LERP_MUTI = 24f;
		private const float CONFLICT_MOVING_TIME = 0.08f;




		#region --- EDT ---

#if UNITY_EDITOR

		private void Reset () {

			// Init Legs
			List<Leg> legs = new List<Leg>(transform.parent.GetComponentsInChildren<Leg>(false));
			if (legs.Contains(this)) {
				legs.Remove(this);
			}
			List<Leg> LegsL = new List<Leg>();
			List<Leg> LegsR = new List<Leg>();
			for (int i = 0; i < legs.Count; i++) {
				if (legs[i].transform.localPosition.x > 0) {
					LegsR.Add(legs[i]);
				} else {
					LegsL.Add(legs[i]);
				}
			}
			int minID = 0;
			float minDistance = float.MaxValue;
			for (int i = 0; i < LegsL.Count; i++) {
				float d = Vector3.Distance(transform.localPosition, LegsL[i].transform.localPosition);
				if (d < minDistance) {
					minID = i;
					minDistance = d;
				}
			}
			if (LegsL.Count > minID) {
				ConflictLegA = LegsL[minID];
			}

			minID = 0;
			minDistance = float.MaxValue;
			for (int i = 0; i < LegsR.Count; i++) {
				float d = Vector3.Distance(transform.localPosition, LegsR[i].transform.localPosition);
				if (d < minDistance) {
					minID = i;
					minDistance = d;
				}
			}
			if (LegsR.Count > minID) {
				ConflictLegB = LegsR[minID];
			}

		}


#endif

		#endregion




		private void Start () {
			InitLocalPos = transform.localPosition;
			InitLocalRot = transform.localRotation;
			AimPos = InitLocalPos;
			PrevPos = transform.position;
			PrevRot = transform.rotation;
			Chr = GetComponentInParent<CharacterController>();
		}



		private void Update () {

			// Dis
			float dis = Distance;

			// When this leg is moving
			if (Moving) {

				// Lerp move position
				Vector3 pos = Vector3.Lerp(
					transform.localPosition,
					AimPos,
					Time.deltaTime * LERP_MUTI
				);
				pos.y = InitLocalPos.y;
				transform.localPosition = pos;

				// Lerp move rotation
				transform.localRotation = Quaternion.Lerp(
					transform.localRotation,
					InitLocalRot,
					Time.deltaTime * LERP_MUTI
				);

			} else {
				// When this leg isn't moving
				if (Sliping && (!Chr || Chr.velocity.magnitude > 0.1f)) {
					// Slip leg
					Vector3 newPos = PrevPos + Random.insideUnitSphere * 0.2f;
					newPos.y = PrevPos.y;
					transform.position = newPos;
				} else {
					// Legs don't move with body
					transform.position = PrevPos;
				}

				transform.rotation = PrevRot;

				dis = Distance;

				// When need to move the foot
				if (dis > StepDistance) {
					// When conflict legs are not moving
					if (!ConflictLegA.Moving && !ConflictLegB.Moving) {
						if (ConflictLegA.Distance < dis && ConflictLegB.Distance < dis) {
							// Move this leg
							MovingTime = Time.time;
							transform.localScale = Vector3.one * 1.5f;
							AimPos = InitLocalPos + (InitLocalPos - transform.localPosition).normalized * StepLength;
						}
					}
				}
			}

			// When this leg out of max distance
			if (dis > SlideDistance) {
				// Slide this leg
				Vector3 pos = InitLocalPos + Vector3.ClampMagnitude(transform.localPosition - InitLocalPos, SlideDistance);
				pos.y = InitLocalPos.y;
				transform.localPosition = pos;
			}

			// Lerp scale
			transform.localScale = Vector3.Lerp(
				transform.localScale,
				Vector3.one,
				Time.deltaTime * LERP_MUTI * 0.4f
			);

			// Lock Y
			transform.localPosition = new Vector3(
				transform.localPosition.x,
				InitLocalPos.y,
				transform.localPosition.z
			);

			// Cache
			PrevPos = transform.position;
			PrevRot = transform.rotation;
		}





	}

}

