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

	// TODO: Store civlians' PoliceAvoidanceBehavior in the OnTriggerEnter for making policeDetected=false when disabling the Emitter

	public bool Enabled
	{
		get => _enabled;
		set
		{
			// Obtaining components if not available
			if (!sirenAudioSource)
				sirenAudioSource = GetComponent<AudioSource>();
			if (!Emitter)
				Emitter = GetComponent<Collider>();

			// If there is a real change
			if ((_enabled != value))
			{
				_enabled = value;

				// If activating
				if (_enabled)
				{
					sirenAudioSource.time = Random.Range(0f, sirenAudioSource.clip.length);
					sirenAudioSource.Play();
				}
				// If deactivating
				else
					sirenAudioSource.time = sirenAudioSource.clip.length * audioRatioAtDeactivation;

				// In any case
				sirenAudioSource.enabled = _enabled;
				sirenAudioSource.loop = _enabled;
				Emitter.enabled = _enabled;
				redLight1.SetActive(_enabled);
				redLight2.SetActive(_enabled);
				blueLight1.SetActive(_enabled);
				blueLight2.SetActive(_enabled);
			}
		}
	}
	private bool _enabled;

	// Start is called before the first frame update
	void Start()
	{
		sirenAudioSource = GetComponent<AudioSource>();
		Emitter = GetComponent<Collider>();
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

}
