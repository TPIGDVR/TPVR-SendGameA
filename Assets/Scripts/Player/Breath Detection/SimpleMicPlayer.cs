using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMicPlayer : MonoBehaviour
{
    [SerializeField] AudioSource _audiosource;
    [SerializeField] Caress.SampleRate _caressSampleRate = Caress.SampleRate._48000;
    [Range(1,30)]
    [SerializeField] private int length = 5;
    private void Start()
    {
        foreach (string name in Microphone.devices)
        {
            print(name);
        }
        var clip = Microphone.Start(null, true, length, (int)_caressSampleRate);
        _audiosource.clip = clip;
        _audiosource.loop = true;
        while(!(Microphone.GetPosition(null) > 0))
        _audiosource.Play();
    }
}
