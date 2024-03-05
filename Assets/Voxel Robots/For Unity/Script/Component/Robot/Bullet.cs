namespace MoenenGames.VoxelRobot {
	using UnityEngine;
	using System.Collections;


	[RequireComponent(typeof(Rigidbody))]
	public class Bullet : MonoBehaviour {




		#region --- VAR ---


		// Shot Cut

		public Transform Shooter {
			get; set;
		}

		public float Damage {
			get; set;
		}

		public Rigidbody Rig {
			get {
				if (!rig) {
					rig = GetComponent<Rigidbody>();
				}
				return rig;
			}
		}

		public Collider Col {
			get {
				if (!col) {
					col = GetComponent<Collider>();
				}
				return col;
			}
		}

		public Bullet Copy {
			get {
				return Instantiate(gameObject).GetComponent<Bullet>();
			}
		}


		// Serialize
		[Header("Setting")]
		[SerializeField] private float LifeTime = 1f;

		[Header("On Hit")]
		[SerializeField] private bool DontDestroyOnHit = false;
		[SerializeField] private bool StopOnHit = false;

		[Header("Component")]
		[SerializeField] private ParticleSystem Particle = null;
		[SerializeField] private Transform Model = null;


		// Data
		[HideInInspector]
		public bool Alive = false;
		private Rigidbody rig = null;
		private Collider col = null;

		// Setting
		private const float BULLET_MAX_REBOUND_SPEED = 20f;



		#endregion




		#region --- MSG ---




		void Start () {
			// Self Kill
			Invoke("DisableCollider", LifeTime);
			Invoke("DestoryBullet", LifeTime + 1f);
			// Size
			SetSize(transform.localScale.x);
		}




		void OnCollisionEnter (Collision col) {
			if (col.collider.isTrigger) {
				return;
			}
			Colliding(col.transform);
		}




		void OnTriggerEnter (Collider c) {
			if (c.isTrigger) {
				return;
			}
			Colliding(c.transform);
		}




		#endregion





		#region --- LGC ---



		void Colliding (Transform tf) {

			if (!Alive) {
				return;
			}

			// Damage

			if (Shooter == tf) {
				return;
			}


			if (!DontDestroyOnHit) {

				// Logic
				Alive = false;

				// Stop
				if (StopOnHit) {
					Rig.velocity = Vector3.zero;
				} else {
					Rig.velocity = Vector3.ClampMagnitude(Rig.velocity, BULLET_MAX_REBOUND_SPEED);
				}

				// Particle
				if (Particle) {
					Particle.Play();
				}

				// Col
				DisableCollider();

				// System
				CancelInvoke();
				DestoryBullet();

			}


		}



		private void DisableCollider () {
			Alive = false;
			if (Col) {
				Col.enabled = false;
			}
		}



		private void DestoryBullet () {
			Alive = false;
			TrailRenderer t = GetComponent<TrailRenderer>();
			if (t) {
				t.enabled = false;
			}
			if (Model) {
				Model.gameObject.SetActive(false);
			}

			// Destroy
			Destroy(gameObject, Particle ? Particle.main.duration + Particle.main.startLifetimeMultiplier : 0.1f);
		}



		private void SetSize (float size) {

			// Trail
			TrailRenderer t = transform.GetComponent<TrailRenderer>();
			if (t) {
				t.widthMultiplier = size;
			}

			// Rig
			Rig.mass *= size;

		}



		#endregion



	}
}