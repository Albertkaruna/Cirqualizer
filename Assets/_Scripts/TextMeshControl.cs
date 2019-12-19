using UnityEngine;
using System.Collections;

public class TextMeshControl : MonoBehaviour
{
	[ExecuteInEditMode]
	// Use this for initialization
	void Start ()
	{
		GetComponent<MeshRenderer> ().sortingLayerName = "Foreground";
		GetComponent<TextMesh> ().text = (transform.parent.transform.localScale.x * 100).ToString ();
	}
}
