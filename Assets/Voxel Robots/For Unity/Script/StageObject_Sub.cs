namespace MoenenGames.VoxelRobot {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;



	[System.Serializable]
	public class BasicInfo {

		public string Name = "";
		public Sprite Icon;
		public StageSex Sex = StageSex.Unknown;
		public StageTeam Team = StageTeam.None;


		#region --- EDT ---

#if UNITY_EDITOR

		public void Reset (Transform tf) {
			// Name
			Name = tf.name;
		}

#endif

		#endregion


	}




	[System.Serializable]
	public class Health {


		[Header("Basic")]
		public float FullHP = 100f;
		public bool Invincible = false;

		[Header("Protection")]
		[Range(0f, 1f)]
		[SerializeField]
		private float MainProtection = 0f;
		[Range(0f, 1f)]
		[SerializeField]
		private float PhysicsProtection = 0f;
		[Range(0f, 1f)]
		[SerializeField]
		private float FireProtection = 0f;
		[Range(0f, 1f)]
		[SerializeField]
		private float IceProtection = 0f;
		[Range(0f, 1f)]
		[SerializeField]
		private float ElecProtection = 0f;
		[Range(0f, 1f)]
		[SerializeField]
		private float RadiationProtection = 0f;



		public float GetFixedProtect (DamageType type) {
			switch (type) {
				default:
				case DamageType.Physics:
					return (1f - MainProtection) * (1f - PhysicsProtection);
				case DamageType.Fire:
					return (1f - MainProtection) * (1f - FireProtection);
				case DamageType.Ice:
					return (1f - MainProtection) * (1f - IceProtection);
				case DamageType.Elec:
					return (1f - MainProtection) * (1f - ElecProtection);
				case DamageType.Radiation:
					return (1f - MainProtection) * (1f - RadiationProtection);
			}
		}



		#region --- EDT ---
#if UNITY_EDITOR
		public void Reset () {
			FullHP = 100f;
			PhysicsProtection = 0f;
			FireProtection = 0f;
			IceProtection = 0f;
			ElecProtection = 0f;
			RadiationProtection = 0f;
		}
#endif
		#endregion



	}




	[System.Serializable]
	public class Breakable {


		public bool DestroyOnDied {
			get {
				return destroyOnDied;
			}
		}


		[Header("Setting")]
		[SerializeField]
		private bool BlowOutOnDied = true;
		[SerializeField]
		private bool destroyOnDied = true;
		[SerializeField]
		private float ColliderSize = 0.2f;
		[SerializeField]
		private float DestroyTime = 5f;
		[SerializeField]
		private float ExplosionIntensity = 50f;

		[Header("Component")]
		[SerializeField]
		private ParticleSystem[] DieParticles;




		public void PlayParticles () {
			for (int i = 0; i < DieParticles.Length; i++) {
				DieParticles[i].Play();
			}
		}





		public void BlowOut (Transform root) {

			if (!BlowOutOnDied) {
				return;
			}

			MeshRenderer[] mrs = root.GetComponentsInChildren<MeshRenderer>(false);
			for (int i = 0; i < mrs.Length; i++) {

				// Transform
				Transform tf = Object.Instantiate(mrs[i].gameObject).transform;
				tf.SetParent(null);
				tf.position = mrs[i].transform.position;
				tf.rotation = mrs[i].transform.rotation;
				tf.localScale = mrs[i].transform.localScale;
				Object.Destroy(tf.gameObject, DestroyTime + 1f);

				// Add Collider
				SphereCollider c = tf.gameObject.AddComponent<SphereCollider>();
				c.radius = ColliderSize;
				Object.Destroy(c, DestroyTime);

				// Add Rigidbody
				Rigidbody rig = tf.gameObject.AddComponent<Rigidbody>();
				rig.mass = 0.1f;
				rig.angularDrag = 6f;

				// Add Explosion
				Vector3 dir = Random.insideUnitSphere;
				dir.y = 1f;
				rig.AddExplosionForce(
					ExplosionIntensity,
					tf.position - dir,
					dir.magnitude * 2f
				);

				// Rotate
				rig.angularVelocity = Random.insideUnitSphere * Random.Range(-100f, 100f);

			}

		}




		#region --- EDT ---


#if UNITY_EDITOR

		public void Reset (Transform root) {
			DieParticles = new ParticleSystem[0];
		}

#endif

		#endregion




	}


}