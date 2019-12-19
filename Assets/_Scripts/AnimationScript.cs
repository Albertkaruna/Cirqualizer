using UnityEngine;
using System.Collections;

public class AnimationScript : MonoBehaviour
{

	public void HideIns ()
	{
		UIManager.instance.instruction.SetActive (false);
	}
}
