using UnityEngine;

namespace Manus.Haptics
{
	/// <summary>
	/// This is the class which needs to be on every hand with haptics in order for all the other haptic related components to function correctly.
	/// In order for the haptics to function each of the fingers on the hand will need a FingerHaptics class with the correct finger type set.
	/// The FingerHaptics will generate haptic values for this class to give to the hand.
	/// </summary>
	[DisallowMultipleComponent]
	[AddComponentMenu("Manus/Haptics/Hand (Haptics)")]
	public class HandHaptics : MonoBehaviour
	{
		Hand.Hand m_Hand;
		FingerHaptics[] m_Fingers;
		public ChooseScript chooseScript;
		public LowerScript lowerScript;

		void Start()
		{
			m_Hand = GetComponentInParent<Hand.Hand>();
			m_Fingers = GetComponentsInChildren<FingerHaptics>();
		}

		void FixedUpdate()
		{
			if (m_Hand.data == null) return;
			for (int i = 0; i < m_Fingers.Length; i++)
			{
				if(i==0) m_Hand.data.SetFingerHaptic(m_Fingers[i].fingerType, FindObjectOfType<ChooseScript>().PassData());
				//m_Hand.data.SetFingerHaptic(m_Fingers[i].fingerType, FindObjectOfType<ChooseScript>().PassData());
				//m_Hand.data.SetFingerHaptic(m_Fingers[i].fingerType, FindObjectOfType<LowerScript>().PassData());
			}
		}
	}
}
