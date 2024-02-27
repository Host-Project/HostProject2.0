namespace MoenenGames.VoxelRobot {
	using UnityEngine;
	using System.Collections;


	[DisallowMultipleComponent]
	public class RobotMovement : CharacterMovement {




		#region --- VAR ---




		// Cache
		private Leg[] Legs;
		private float LegSlipTime = float.MinValue;
		private bool PrevLegSliping = false;



		#endregion




		#region --- MSG ---



		protected override void Awake () {
			base.Awake();
			PrevLegSliping = true;
			Legs = GetComponentsInChildren<Leg>(true);
			SlipingForAllLegs(false);
		}



		protected override void Update () {
			base.Update();
			LegSlipUpdate();
		}




		#endregion




		#region --- LGC ---



		private void SlipingForAllLegs (bool slip) {
			if (PrevLegSliping != slip) {
				PrevLegSliping = slip;
				for (int i = 0; i < Legs.Length; i++) {
					Legs[i].Sliping = slip;
				}
			}
		}



		private void LegSlipUpdate () {
			if (Time.time < LegSlipTime) {
				SlipingForAllLegs(AimVelocity.magnitude > 2f || Chr.velocity.magnitude > 2f);
			} else {
				MoveLerpRate = 1f;
				SlipingForAllLegs(false);
			}
		}




		#endregion




	}
}