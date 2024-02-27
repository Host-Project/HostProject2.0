namespace MoenenGames.VoxelRobot {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;


	public class Demo : MonoBehaviour {



		// VAR
		private Transform ShowingItemRoot {
			get {
				if (!m_ShowingItemRoot) {
					m_ShowingItemRoot = new GameObject("Showing Item Root").transform;
					m_ShowingItemRoot.SetParent(null);
					m_ShowingItemRoot.position = Vector3.zero;
					m_ShowingItemRoot.rotation = Quaternion.identity;
					m_ShowingItemRoot.localScale = Vector3.one;
				}
				return m_ShowingItemRoot;
			}
		}


		[SerializeField] private List<Transform> Data = null;



		private int CurrentItemIndex = 0;
		private Transform m_ShowingItemRoot = null;
		private string Name = "";
		private bool ToPrev = false;
		private bool ToNext = false;


		// MSG
		private void OnGUI () {

			int width = Screen.width;
			int height = Screen.height;
			float itemHeight = height * 0.05f;
			float buttonWidth = width * 0.1f;

			GUI.Label(new Rect(width / 2f, itemHeight, width, itemHeight), Name);
			if (GUI.Button(new Rect(0, 0, buttonWidth, itemHeight), "<")) {
				ToPrev = true;
			}
			if (GUI.Button(new Rect(buttonWidth * 1.05f, 0, buttonWidth, itemHeight), ">")) {
				ToNext = true;
			}
		}


		private void Awake () {
			SwitchItem();
		}



		private void Update () {

			if (
				ToPrev || Input.GetKeyDown(KeyCode.Backspace)
			) {
				ToPrev = false;
				SpawnPrevItem();
			}


			if (
				ToNext ||
				Input.GetKeyDown(KeyCode.Space) ||
				Input.GetKeyDown(KeyCode.Return)
			) {
				ToNext = false;
				SpawnNextItem();
			}

		}



		private void SwitchItem () {

			// Check
			if (Data.Count == 0 || Data[CurrentItemIndex] == null) {
				Name = "";
				return;
			}

			// Name
			ResetName();

			// Play
			RespawnItem(CurrentItemIndex);

		}




		public void RespawnItem (int index) {

			// Delete Old
			if (m_ShowingItemRoot) {
				DestroyImmediate(m_ShowingItemRoot.gameObject, false);
			}

			// Add New
			Transform tf = Instantiate(Data[index]);
			tf.SetParent(ShowingItemRoot);
			tf.position = Vector3.up * 2f;
			tf.rotation = Quaternion.identity;
			tf.localScale = Vector3.one;

		}



		// API
		public void UGUI_NextItem () {
			SpawnNextItem();
		}



		public void UGUI_PrevItem () {
			SpawnPrevItem();
		}



		public void UGUI_FiveStar () {
			Application.OpenURL(@"http://u3d.as/MKK");
		}






		// Logic
		private void SpawnNextItem () {
			CurrentItemIndex = (int)Mathf.Repeat(CurrentItemIndex + 1, Data.Count);
			SwitchItem();
		}


		private void SpawnPrevItem () {
			CurrentItemIndex = (int)Mathf.Repeat(CurrentItemIndex - 1, Data.Count);
			SwitchItem();
		}


		private void ResetName () {
			string _name = Data[CurrentItemIndex].name;
			Name = _name + "\n<size=18>" + (CurrentItemIndex + 1).ToString("00") + " / " + Data.Count.ToString("00") + "</size>";
		}


	}
}