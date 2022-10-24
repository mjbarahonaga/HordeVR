using UnityEngine;
using System.Collections;

public class Door : MonoBehaviour {

	Animator m_Animator;


	void Start ()
	{
		m_Animator = GetComponent<Animator>();
	}

	void OnTriggerEnter (Collider other)
	{
		m_Animator.SetBool("Open", true);
		GetComponent<AudioSource>().Play();
	}
	void OnTriggerExit (Collider other)
	{
		m_Animator.SetBool("Open", false);	
	}
}



