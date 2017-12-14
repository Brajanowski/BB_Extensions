using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Transform))]
public class PlaceOnGround : Editor {
	void OnSceneGUI() {
		Handles.BeginGUI();

		GUILayout.BeginArea(new Rect(5, 5, 160, 24));

		if (GUILayout.Button("Place on the ground")) {
			Place((Transform)target);	
		}

		GUILayout.EndArea();

		Handles.EndGUI();
	}

	static void Place(Transform transform) {
		RaycastHit hit;
		if (Physics.Raycast(transform.position, Vector3.down, out hit)) {
			transform.position = hit.point;
			
			Collider collider = transform.GetComponent<Collider>();
			if (collider != null) {
				transform.position = new Vector3(transform.position.x, transform.position.y + collider.bounds.extents.y, transform.position.z);	
			}
		}
	}
}
