using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ButtonAnimationScript : MonoBehaviour {

	public void Start()
	{
		Color BaseColor = GetComponent<Image> ().color;

		RuntimeAnimatorController controller = GetComponent<Animator> ().runtimeAnimatorController;

		AnimatorOverrideController overrideController = new AnimatorOverrideController ();
		overrideController.name = "ModifiedController";
		overrideController.runtimeAnimatorController = controller;

		//AnimationClip[] clips = controller.animationClips;

		Color highlightColor = UI.UICore.instance.getHighlightColorFor (BaseColor);
		Color pressedColor = UI.UICore.instance.getPressedColorFor (BaseColor);

		AnimationCurve curve;

		AnimationClip newNormalClip = new AnimationClip ();
		newNormalClip.name = "Normal";
		curve = new AnimationCurve (new Keyframe (0, BaseColor.r));
		newNormalClip.SetCurve ("", typeof(Image), "m_Color.r", curve);
		curve = new AnimationCurve (new Keyframe (0, BaseColor.g));
		newNormalClip.SetCurve ("", typeof(Image), "m_Color.g", curve);
		curve = new AnimationCurve (new Keyframe (0, BaseColor.b));
		newNormalClip.SetCurve ("", typeof(Image), "m_Color.b", curve);
		curve = new AnimationCurve (new Keyframe (0, BaseColor.a));
		newNormalClip.SetCurve ("", typeof(Image), "m_Color.a", curve);

		AnimationClip newHighlightClip = new AnimationClip ();
		newHighlightClip.name = "Highlighted";
		curve = new AnimationCurve (new Keyframe (0, highlightColor.r));
		newHighlightClip.SetCurve ("", typeof(Image), "m_Color.r", curve);
		curve = new AnimationCurve (new Keyframe (0, highlightColor.g));
		newHighlightClip.SetCurve ("", typeof(Image), "m_Color.g", curve);
		curve = new AnimationCurve (new Keyframe (0, highlightColor.b));
		newHighlightClip.SetCurve ("", typeof(Image), "m_Color.b", curve);
		curve = new AnimationCurve (new Keyframe (0, highlightColor.a));
		newHighlightClip.SetCurve ("", typeof(Image), "m_Color.a", curve);

		AnimationClip newPressedClip = new AnimationClip ();
		newPressedClip.name = "Pressed";
		curve = new AnimationCurve (new Keyframe (0, pressedColor.r));
		newPressedClip.SetCurve ("", typeof(Image), "m_Color.r", curve);
		curve = new AnimationCurve (new Keyframe (0, pressedColor.g));
		newPressedClip.SetCurve ("", typeof(Image), "m_Color.g", curve);
		curve = new AnimationCurve (new Keyframe (0, pressedColor.b));
		newPressedClip.SetCurve ("", typeof(Image), "m_Color.b", curve);
		curve = new AnimationCurve (new Keyframe (0, pressedColor.a));
		newPressedClip.SetCurve ("", typeof(Image), "m_Color.a", curve);

		overrideController["Normal"] = newNormalClip;
		overrideController["Highlighted"] = newHighlightClip;
		overrideController["Pressed"] = newPressedClip;

		// Put this line at the end because when you assign a controller on an Animator, unity rebind all the animated properties
		GetComponent<Animator> ().runtimeAnimatorController = overrideController; 
	}
}
