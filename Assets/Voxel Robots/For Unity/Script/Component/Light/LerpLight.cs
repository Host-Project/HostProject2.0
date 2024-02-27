namespace MoenenGames.VoxelRobot {
	using UnityEngine;
	using System.Collections;

	[RequireComponent(typeof(Light))]
	public class LerpLight : MonoBehaviour {



		// Shot Cut
		private Light TheLight {
			get {
				if (!theLight) {
					theLight = GetComponent<Light>();
				}
				return theLight;
			}
		}


		// Serialize
		[SerializeField]
		private float AimIntensity = 0f;
		[SerializeField]
		private float FullIntensity = 8f;
		[SerializeField]
		private float LerpRate = 20f;


		// Data
		private Light theLight = null;
		private bool Moving = true;




		void Update () {
			if (Moving) {
				TheLight.intensity = Mathf.Lerp(TheLight.intensity, AimIntensity, Time.deltaTime * LerpRate);
				if (Mathf.Abs(TheLight.intensity - AimIntensity) < 0.01f) {
					TheLight.intensity = AimIntensity;
					Moving = false;
					if (AimIntensity <= 0f) {
						TheLight.enabled = false;
					}
				}
			}
		}




		public void TriggerOn (float intensity = -1f) {
			Moving = true;
			TheLight.intensity = intensity < 0 ? FullIntensity : intensity;
			TheLight.enabled = true;
		}



		public void SetAimIntensity (float i = 0f) {
			Moving = true;
			TheLight.enabled = true;
			AimIntensity = i;
		}



	}
}