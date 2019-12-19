using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.EventSystems;

/// <summary>
/// Game controller.
/// This script controls the full gaming experience like user input, controlling gameplay, loading new level, etc.
/// </summary>

public class GameController : MonoBehaviour
{
	public static GameController instance;

    public static int adCount = 0;

	[SerializeField]
	private Transform levelsParent;
	// All levels for the complete Gameplay
	[HideInInspector]
	public GameObject[] _levels;
	// Ray hit variable to find the hit object
	private RaycastHit2D hit;
	// Temporary collider to hold the specific object
	private Collider2D collider1;
	// num variable used to store the current circle's value
	private int num = 0, num1 = 0;
	// Just temporary varialbes to check the Threshold values
	private float temp1, temp2;
	// Values used to Scale up the circle, Scale down the Text object and increase the font size
	private float mainScale = 0.01f, textScale = 0.003f, fontThreshold = 0.01f, numThreshold = 0.01f;
	// Array of TextMeshs at the current Level
	private TextMesh[] textMeshes;
	// Store the list of all Circle's Text values to calculate the equality of all of 'em
	private List<int> list = new List<int> ();
	// Counter for Number of levels
	[HideInInspector]
	public int lvl_Count = 0;
	//Listss used to store the required dataas for resetting purpose
	private List<float> circleScaleValue = new List<float> ();
	private List<float> textScaleValue = new List<float> ();
	private List<int> fontSizeValue = new List<int> ();
	private List<int> levelCountForFade = new List<int> ();
	private bool canLetInput = false;

	void Awake ()
	{
		if (instance != null) {
			Destroy (gameObject);
		} else {
			instance = this;
			DontDestroyOnLoad (gameObject);
		}
	}


	// Use this for initialization
	void Start ()
	{
		_levels = new GameObject[levelsParent.childCount];
		for (int i = 0; i < levelsParent.childCount; i++) {
			_levels [i] = levelsParent.GetChild (i).gameObject;
		}
		lvl_Count = PreferenceController.instance.GetLevelCount ();
        print("Levelcount: " + lvl_Count);
		//		 Get new level at first
		GetNewLevel ();
	}
	
	// Update is called once per frame
	void Update ()
	{
//		Get the user input by using RayCasting
		Vector2 worldPoint = Camera.main.ScreenToWorldPoint (Input.mousePosition);		
		hit = Physics2D.Raycast (worldPoint, Vector2.zero, 1f);
		if (hit.collider != null && Input.GetMouseButton (0) && canLetInput) {
			collider1 = hit.collider;
			if (collider1.tag == "Circle" && !collider1.GetComponent<BorderControl> ().isItJoined && !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) {
				ManageSize ();
			} else if (collider1.tag == "Circle" && collider1.GetComponent<BorderControl> ().isItJoined &&  !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) 
            {   
                ManageSizeIfJoined ();
			}

			if (PreferenceController.instance.GetHideIns () == 0) {
				UIManager.instance.instruction.GetComponent<Animator> ().enabled = true;
                Invoke("HideIns", 1.5f);
				PreferenceController.instance.SetHideIns (1);
			}
		}
		if (Input.GetMouseButtonUp (0)) {
			CheckEquality ();
		}       
	}


    void HideIns()
    {
        UIManager.instance.instruction.SetActive(false);
    }


	// Get the level to play
	public void GetNewLevel ()
	{
       // lvl_Count = 10; // Remove this line
        if (lvl_Count < _levels.Length) {
			_levels [lvl_Count].SetActive (true);
			if (levelCountForFade.Contains (lvl_Count)) {
				iTween.FadeTo (_levels [lvl_Count], 1, 1f);
			} else {
				iTween.FadeFrom (_levels [lvl_Count], 0, 1f);
				if (!levelCountForFade.Contains (lvl_Count)) {
					levelCountForFade.Add (lvl_Count);
				}
			}
			Invoke ("LetOrStopInput", 0.5f);
			GetRequiredDatas ();
		} else {
			print ("No More Levels, Probably Game End !");
		}
	}



	// Increase the Circle and also decrease the TextMesh gameobject and increase the TextMesh text
	void ManageSize ()
	{
		Transform childrn;
		childrn = collider1.transform.GetChild (0).transform;
		num = int.Parse (childrn.GetComponent<TextMesh> ().text);
		temp1 += 0.01f;
		temp2 += 0.01f;
		collider1.transform.localScale += Vector3.one * mainScale;
		childrn.localScale -= Vector3.one * textScale;
		if (temp1 >= fontThreshold) {
			temp1 = 0f;
			childrn.GetComponent<TextMesh> ().fontSize += 1;
		}
		if (temp2 >= numThreshold) {
			temp2 = 0;
			num += 1;
			childrn.GetComponent<TextMesh> ().text = num.ToString ();
			if (childrn.transform.localScale.x <= 0f) {
				collider1.enabled = false;
			}
		}
	}
	//Increase the Circle and also decrease the TextMesh gameobject and increase the TextMesh text when 2 or more circles are joined
	void ManageSizeIfJoined ()
	{
		Transform childrnA, childrnB;
		BorderControl bc = collider1.GetComponent<BorderControl> ();
		GameObject[] OtherCircles = new GameObject[bc.joinedCircle.Length];
		for (int k = 0; k < bc.joinedCircle.Length; k++) {
			OtherCircles [k] = bc.joinedCircle [k];

			childrnA = collider1.transform.GetChild (0).transform;
			childrnB = OtherCircles [k].transform.GetChild (0).transform;

			num = int.Parse (childrnA.GetComponent<TextMesh> ().text);
			num1 = int.Parse (childrnB.GetComponent<TextMesh> ().text);
			temp1 += 0.01f;
			temp2 += 0.01f;

			collider1.transform.localScale += Vector3.one * mainScale;
			OtherCircles [k].transform.localScale -= Vector3.one * mainScale;

			childrnA.localScale += Vector3.one * textScale;
			childrnB.localScale -= Vector3.one * textScale;

			if (temp1 >= fontThreshold) {
				temp1 = 0f;
				childrnA.GetComponent<TextMesh> ().fontSize += 1;
				childrnB.GetComponent<TextMesh> ().fontSize -= 1;
			}
			if (temp2 >= numThreshold) {
				temp2 = 0;
				num += 1;
				num1 -= 1;
				childrnA.GetComponent<TextMesh> ().text = num.ToString ();
				childrnB.GetComponent<TextMesh> ().text = num1.ToString ();

				if (childrnB.transform.localScale.x <= 0f) {
					OtherCircles [k].GetComponent<Collider2D> ().enabled = false;
				}
			}
		}
	}

	// Add all Circle's text values as integer to list
	void CheckEquality ()
	{
		foreach (TextMesh tm in textMeshes) {
			list.Add (int.Parse (tm.text));
		}
		CheckVictory ();
	}

	// Once list is full check does all circle's text values are equal, if so then it's Victory
	void CheckVictory ()
	{
		int k = 0;
		for (int i = 0; i < list.Count; i++) {
			if (list [0] == list [i]) {
				k++;
				if (k == list.Count) {
					Victory ();
					print ("Victory");
				}
			}
		}
		list.Clear ();
	}

	// Player completes the level get new level
	public void Victory ()
	{
        adCount++;   
		try {
			iTween.FadeTo (_levels [lvl_Count], iTween.Hash ("a", 0, "time", 0.7f));
			StartCoroutine (DisableOld_AndGet_NewLevel ());
		} catch (System.Exception ex) {
			Debug.Log (ex);
		}	
		LetOrStopInput ();
        lvl_Count++;
        if (lvl_Count >= PreferenceController.instance.GetLevelCount ()) {
			PreferenceController.instance.SetLevelCount (lvl_Count);
        }

        UIManager.instance.PlayVictorySound();
    }
	
	// Load new level if the level was won and disables the old level
	IEnumerator DisableOld_AndGet_NewLevel ()
	{
		yield return new WaitForSeconds (0.7f);
		ResetAllDatas ();
		yield return new WaitForSeconds (0.3f);
		ClearAll ();
		try {
			_levels [lvl_Count - 1].SetActive (false);
		} catch (System.Exception ex) {
			Debug.Log (ex);
		}
		// Get new level
		GetNewLevel ();	
	}

	// Get all required datas about the level
	void GetRequiredDatas ()
	{
		textMeshes = GameObject.FindObjectsOfType (typeof(TextMesh)) as TextMesh[];

		foreach (TextMesh tm in textMeshes) {
			circleScaleValue.Add (tm.transform.parent.transform.localScale.x);
			textScaleValue.Add (tm.transform.localScale.x);
			fontSizeValue.Add (tm.fontSize);
		}
	}

	// Resetting the current level
	public void FailureReset ()
    {
        UIManager.instance.PlayFailureSound();

        adCount++;
        print("Failure");
		ResetAllDatas ();
		ClearAll ();
		BorderControl.once = true;
		iTween.FadeTo (_levels [lvl_Count], 1, 1f);
		Invoke ("LetOrStopInput", 0.5f);
		GetRequiredDatas ();
	}


	void ResetAllDatas ()
	{
		int i = 0;
		foreach (TextMesh tm in textMeshes) {
			tm.transform.parent.transform.localScale = Vector3.one * circleScaleValue [i];
			tm.transform.localScale = Vector3.one * textScaleValue [i];
			tm.fontSize = fontSizeValue [i];
			tm.text = (tm.transform.parent.transform.localScale.x * 100).ToString ();
			i++;
		}
	}

	// Enable or Disable the user input
	public void LetOrStopInput ()
	{
		canLetInput = !canLetInput;
	}

	// Clears all the List values
	void ClearAll ()
	{
		circleScaleValue.Clear ();
		textScaleValue.Clear ();
		fontSizeValue.Clear ();
		list.Clear ();
	}

	// Get the new level when the 'Next' or 'Previous' buttons are pressed
	public void GetNewLevelUI (string s)
	{
		UIManager.instance.NextBtn.interactable = false;
		UIManager.instance.PreviousBtn.interactable = false;

		Invoke ("ResetAllDatas", 0.6f);
		Invoke ("ClearAll", 0.8f);
		//		Turn off input
		LetOrStopInput ();
		iTween.FadeTo (_levels [lvl_Count], iTween.Hash ("a", 0, "time", 1f));
		StartCoroutine (GetNewLevelNow (s));
	}

	// Disables the current level and gets the new level
	IEnumerator GetNewLevelNow (string s)
	{
		int temp = 0;
		temp = lvl_Count;
		if (s == "Next") {
			lvl_Count += 1;
		} else if (s == "Previous") {
			lvl_Count -= 1;
		}
		yield return new WaitForSeconds (1f);
		try {
			_levels [temp].SetActive (false);
			_levels [lvl_Count].SetActive (true);
		} catch (System.Exception ex) {
			Debug.Log (ex);
		}
		iTween.FadeTo (_levels [lvl_Count], iTween.Hash ("a", 1, "time", 1f));
		GetRequiredDatas ();
		yield return new WaitForSeconds (0.5f);
		UIManager.instance.MenuButtonsInteractables ();
		//		Turn on input
		LetOrStopInput ();
	}
}
