using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class BorderControl : MonoBehaviour
{
	public static bool once;

	public bool isItJoined = false;
	public GameObject[] joinedCircle;

	// Use this for initialization
	void Start ()
	{
		once = true;
	}

	void OnTriggerEnter2D (Collider2D col)
	{
		if ((col.tag == "Border" || col.tag == "Circle") && once) {
			once = false;
			GameController.instance.LetOrStopInput ();
			iTween.FadeTo (GameController.instance._levels [PreferenceController.instance.GetLevelCount ()], 0, 1f);
			Invoke ("Restart", 1f);
		}
	}

	void Restart ()
	{
		GameController.instance.FailureReset ();
	}
}
