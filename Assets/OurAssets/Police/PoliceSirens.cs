using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Collider))]
public class PoliceSirens : MonoBehaviour
{
	public GameObject redLight1, redLight2, blueLight1, blueLight2;
	public float waitTime = 0.05f;
	public float audioRatioAtDeactivation = 0.95f;

	private float changeTime = 0.0f;
	private int state = 0;
	private AudioSource sirenAudioSource;
	private Collider Emitter;
	private HashSet<PoliceAvoidanceBehavior> policeAvoidanceBehaviors;

	public bool Enabled
	{
		get => _enabled;
		set
		{
			_enabled = value;

			// Obtaining components if not available
			if (!sirenAudioSource)
				sirenAudioSource = GetComponent<AudioSource>();
			if (!Emitter)
				Emitter = GetComponent<Collider>();
			if (policeAvoidanceBehaviors == null)
				policeAvoidanceBehaviors = new HashSet<PoliceAvoidanceBehavior>();

			// If activating
			if (_enabled)
			{
				sirenAudioSource.time = Random.Range(0f, sirenAudioSource.clip.length);
				sirenAudioSource.Play();
			}
			// If deactivating
			else
			{
				sirenAudioSource.time = sirenAudioSource.clip.length * audioRatioAtDeactivation;
				foreach (PoliceAvoidanceBehavior bhv in policeAvoidanceBehaviors)
					bhv.ResetDefault();
				policeAvoidanceBehaviors.Clear();
			}

			// In any case
			sirenAudioSource.loop = _enabled;
			Emitter.enabled = _enabled;
			redLight1.SetActive(_enabled);
			redLight2.SetActive(_enabled);
			blueLight1.SetActive(_enabled);
			blueLight2.SetActive(_enabled);
		}
	}
	private bool _enabled;

	// Start is called before the first frame update
	void Start()
	{
		sirenAudioSource = GetComponent<AudioSource>();
		Emitter = GetComponent<Collider>();
		policeAvoidanceBehaviors = new HashSet<PoliceAvoidanceBehavior>();
	}

	void Update()
	{
		if (Enabled)
		{
			// Player if stopped
			if (!sirenAudioSource.isPlaying)
			{
				sirenAudioSource.time = Random.Range(0f, sirenAudioSource.clip.length);
				sirenAudioSource.Play();
			}

			// Alternating
			changeTime += Time.deltaTime;
			if (changeTime > waitTime)
			{
				changeTime = 0.0f;
				redLight1.SetActive(false);
				redLight2.SetActive(false);
				blueLight1.SetActive(false);
				blueLight2.SetActive(false);

				switch (state)
				{
					case 0: redLight1.SetActive(true); break;
					case 1: redLight2.SetActive(true); break;
					case 2: blueLight1.SetActive(true); break;
					case 3: blueLight2.SetActive(true); break;
					default: break;
				}

				state += 1;
				state %= 4;
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		PoliceAvoidanceBehavior bhv = other.GetComponent<PoliceAvoidanceBehavior>();
		if (bhv)
			policeAvoidanceBehaviors.Add(bhv);
	}

	private void OnTriggerExit(Collider other)
	{
		PoliceAvoidanceBehavior bhv = other.GetComponent<PoliceAvoidanceBehavior>();
		if (bhv)
			policeAvoidanceBehaviors.Remove(bhv);
	}

}
