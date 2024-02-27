namespace MoenenGames.VoxelRobot {
	using UnityEngine;
	using System.Collections;
	using System.Collections.Generic;

	public class Weapon : MonoBehaviour, Controllable {




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

		public Transform Model {
			get {
				return model ? model : transform;
			}
		}

		public float PrevAttackTime {
			get {
				return prevAttackTime;
			}
		}

		public bool ReadyToShoot {
			get {
				int len = ConflictWeapons.Length;
				if (PrevAttackTime + AttackFrequency * (len + 1) > Time.time) {
					return false;
				}
				for (int i = 0; i < len; i++) {
					if (ConflictWeapons[i] == this) {
						continue;
					}
					if (ConflictWeapons[i].PrevAttackTime + AttackFrequency > Time.time) {
						return false;
					}
				}
				return true;
			}
		}


		// Serialize

		[Header("Setting")]
		[SerializeField]
		private float Damage = 1f;
		[SerializeField]
		private float AttackFrequency = 0.4f;
		[SerializeField]
		private float BulletSpeed = 60f;
		[SerializeField]
		private float BulletSize = 0.3f;
		[SerializeField]
		private float RandomTimeGap = 0.05f;
		[SerializeField]
		private bool RotatableY = false;
		[SerializeField]
		private bool LockBulletY = true;

		[Header("System")]
		[SerializeField]
		private AnimationCurve LerpCurveF = AnimationCurve.EaseInOut(0f, -0.4f, 0.4f, 0f);
		[SerializeField]
		private AnimationCurve ScaleCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0f, 1f) });
		[SerializeField]
		private Vector3 CurveRandom = Vector3.zero;
		[SerializeField]
		protected KeyCode AttackKey = KeyCode.Mouse0;

		[Header("Component")]
		[SerializeField] private ParticleSystem[] Particles = new ParticleSystem[0];
		[SerializeField] private Weapon[] ConflictWeapons = null;
		[SerializeField] private Bullet TheBullet = null;
		[SerializeField] private Transform Shooter = null;
		[SerializeField] private LerpLight TheLight = null;
		[SerializeField] private Transform model = null;
		[SerializeField] private Transform bulletSpawnPivot = null;

		// Data
		private float prevAttackTime = float.MinValue;
		private Vector3 InitLocalPos = Vector3.zero;
		private Vector3 CurrentCurveRandom = Vector3.zero;


		#endregion



		#region --- EDT ---

#if UNITY_EDITOR

		void Reset () {

			// Init conflict weapon
			List<Weapon> rws = new List<Weapon>(transform.parent.GetComponentsInChildren<Weapon>(true));
			if (rws.Contains(this)) {
				rws.Remove(this);
			}
			ConflictWeapons = rws.ToArray();

			// Init _m
			Transform _m = transform.Find("_m");
			if (_m) {
				model = _m;
			}

			// Init _s
			Transform _s = transform.Find("_s");
			if (!_s) {
				bulletSpawnPivot = new GameObject("_s").transform;
				bulletSpawnPivot.SetParent(transform);
				bulletSpawnPivot.localPosition = Vector3.zero;
				bulletSpawnPivot.localRotation = Quaternion.identity;
				bulletSpawnPivot.localScale = Vector3.one;
			} else {
				bulletSpawnPivot = _s;
			}

			// Init Light
			LerpLight _l = transform.parent.GetComponentInChildren<LerpLight>(true);
			if (_l) {
				TheLight = _l;
			}

			// Init Particles
			ParticleSystem[] pss = GetComponentsInChildren<ParticleSystem>(true);
			Particles = pss;

		}

#endif

		#endregion



		#region --- MSG ---



		void Awake () {
			InitLocalPos = Model.localPosition;
			StopAllParticles();
		}



		void Update () {
			KeyUpdate();
			ModelUpdate();
			SpawnPivotUpdate();
		}



		#endregion




		#region --- API ---



		public void Attack () {
			ShootBullet();
			TriggerLoghtOn();
		}



		#endregion




		#region --- LGC ---



		void KeyUpdate () {

			if (Input.GetKey(AttackKey)) {
				if (ReadyToShoot) {
					Attack();
					PlayAllParticles();
					CurrentCurveRandom = new Vector3(
						Random.Range(-CurveRandom.x, CurveRandom.x),
						Random.Range(-CurveRandom.y, CurveRandom.y),
						Random.Range(-CurveRandom.z, CurveRandom.z)
					);
					prevAttackTime = Time.time + Random.Range(0f, RandomTimeGap);
				}
			}
		}



		void ModelUpdate () {
			float t = Time.time - PrevAttackTime;
			Model.localPosition = InitLocalPos + Vector3.forward * LerpCurveF.Evaluate(t) + CurrentCurveRandom;
			Model.localScale = Vector3.one * ScaleCurve.Evaluate(t);
		}



		void SpawnPivotUpdate () {
			if (RotatableY) {
				Vector3 mousePos = GetMouseWorldPosition(transform.position, Vector3.up);
				Vector3 dir = mousePos - bulletSpawnPivot.position + Vector3.up * 0.8f;
				Quaternion rot = Quaternion.LookRotation(dir);
				rot = Quaternion.Euler(
					Mathf.Clamp(rot.eulerAngles.x, 0f, 80f),
					bulletSpawnPivot.rotation.y,
					bulletSpawnPivot.rotation.z
				);
				bulletSpawnPivot.localRotation = rot;
			}
		}



		private void PlayAllParticles () {
			for (int i = 0; i < Particles.Length; i++) {
				Particles[i].Play();
			}
		}



		private void StopAllParticles () {
			for (int i = 0; i < Particles.Length; i++) {
				Particles[i].Stop();
			}
		}



		private void ShootBullet () {

			Bullet b = TheBullet.Copy;
			Transform tf = b.transform;
			Vector3 pos = bulletSpawnPivot.position;
			if (LockBulletY) {
				pos.y = Shooter.transform.position.y + 1f;
			}

			tf.gameObject.SetActive(false);
			tf.position = pos;
			tf.rotation = bulletSpawnPivot.rotation;
			tf.gameObject.SetActive(true);
			tf.localScale = Vector3.one * BulletSize;

			b.Shooter = Shooter;
			b.Damage = Damage;
			b.Rig.velocity = Vector3.ClampMagnitude(bulletSpawnPivot.forward, 1f) * BulletSpeed;
			b.Alive = true;
		}



		private void TriggerLoghtOn () {
			if (TheLight) {
				TheLight.TriggerOn();
			}
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



		#endregion


	}
}