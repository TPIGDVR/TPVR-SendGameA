﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using static Unity.VisualScripting.Member;

namespace Breathing3
{
    public class MicController : MonoBehaviour, VolumeProvider
    {
        [SerializeField] TextMeshProUGUI text;
        AudioSource _audioSource;
        private AudioMixer aMixer;
        
        private float[] _dataContainer;
        private List<float> _pitchContainer;
        private List<float> _volumeContainer;
        [SerializeField] int _pitchRecordTime = 5;
        [SerializeField] int _volumeRecordTime = 5;
        [SerializeField] int _datalength = 1024;
        //mic settings
        private int maxFrequency = 44100;
        private int minFrequency = 0;
        public bool mute = true;

        [Header("Other settings")]
        [SerializeField] private float loudnessMultiplier = 100f; //Multiply loudness with this number
        [SerializeField] private float highPassCutoff = 10000;
        private string _micphoneName;
        private bool isMicrophoneReady;
        #region volume provider implementation
        public float volume { get; set; }
        public float pitch { get; set; }
        public float avgVolume { get; set; }
        public float avgPitch { get; set; }
        public float maxVolume { get; set; }
        public float minVolume { get; set; }
        public float maxPitch { get; set; }
        public float minPitch { get; set; }
        #endregion

        IEnumerator Start()
        {

            aMixer = Resources.Load("MicrophoneMixer") as AudioMixer;
            if (mute)
            {
                aMixer.SetFloat("MicrophoneVolume", -80);
            }
            else
            {
                aMixer.SetFloat("MicrophoneVolume", 0);
            }


            if (Microphone.devices.Length == 0)
            {
                Debug.LogWarning("No microphone detected.");
            }


            //if using Android or iOS -> request microphone permission
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);

                if (!Application.HasUserAuthorization(UserAuthorization.Microphone))
                {
                    Debug.LogWarning("Application does not have microphone permission.");
                    yield break;
                }
            }

            _audioSource = gameObject.AddComponent<AudioSource>();
            prepareMicrophone();
            
            _audioSource.playOnAwake = false;

            _dataContainer = new float[_datalength];
            _pitchContainer = new();
            _volumeContainer = new();
        }

        private void Update()
        {
            CalculateVolume();
            CalculatePitch();

            text.text = $"Volume: {volume}\n" +
                $"Min Volume: {minVolume}\n" +
                $"Max Volume: {maxVolume} \n" +
                $"Avg Volume: {avgVolume} \n" +
                $"Pitch: {pitch}\n" +
                $"Min pitch: {minPitch}\n" +
                $"Max pitch: {maxPitch} \n" +
                $"Avg pitch: {avgPitch}";
        }

        void prepareMicrophone()
        {
            if (Microphone.devices.Length > 0)
            {
                //Gets the maxFrequency and minFrequency of the device
                _micphoneName = Microphone.devices[0];
                Microphone.GetDeviceCaps(_micphoneName, out minFrequency, out maxFrequency);
                if (maxFrequency == 0)
                {//These 2 lines of code are mainly for windows computers
                    maxFrequency = 44100;
                }
                if (_audioSource.clip == null)
                {
                    _audioSource.clip = Microphone.Start(_micphoneName, true, 1, maxFrequency);
                    _audioSource.loop = true;

                    //Wait until microphone starts recording
                    while (!(Microphone.GetPosition(_micphoneName) > 0))
                    {
                    }
                }
                _audioSource.Play();
                isMicrophoneReady = true;

            }
            else
            {
                Debug.LogWarning("No microphone detected.");
            }

        }

        void CalculateVolume()
        {
            _audioSource.GetOutputData(_dataContainer, 0);
            CalculateNormalVolume();
            CalculateMaxMinAverageVolume();


            void CalculateNormalVolume()
            {
                float sum = 0;
                for (int i = 0; i < _dataContainer.Length; i++)
                {
                    sum += Mathf.Pow(_dataContainer[i], 2);//Mathf.Abs(dataContainer[i]);
                }
                volume = Mathf.Sqrt(sum / _datalength) * loudnessMultiplier;
            }
            void CalculateMaxMinAverageVolume()
            {
                _volumeContainer.Add(volume);
                if(_volumeContainer.Count >= _volumeRecordTime)
                {
                    _volumeContainer.RemoveAt(0);
                }

                float minVol = float.MaxValue;
                float maxVol = float.MinValue;
                float avgVol = 0;

                foreach(var vol in _volumeContainer)
                {
                    if(vol < minVol)
                    {
                        minVol = vol;
                    }
                    else if(vol > maxVol)
                    {
                        maxVol = vol;
                    }

                    avgVol += vol;
                }

                minVolume = minVol;
                maxVolume = maxVol;
                avgVolume = avgVol / _volumeContainer.Count;
            }
        }


        void CalculatePitch()
        {
            _audioSource.GetSpectrumData(_dataContainer, 0, FFTWindow.BlackmanHarris);
            CalculateNormalPitch();
            CalculateMinMaxAveragePitch();

            void CalculateNormalPitch()
            {
                float maxV = 0;
                int maxN = 0;

                // Find the highest sample.
                for (int i = 0; i < _dataContainer.Length; i++)
                {
                    if (_dataContainer[i] > maxV)
                    {
                        maxV = _dataContainer[i];
                        maxN = i; // maxN is the index of max
                    }
                }

                // Pass the index to a float variable
                float freqN = maxN;

                // Convert index to frequency
                //24000 is the sampling frequency for the PC. 24000 / sample = frequency resolution
                // frequency resolution * index of the sample would give the pitch as a result.
                pitch = HighPassFilter(freqN * 24000 / _datalength, highPassCutoff);

            }

            void CalculateMinMaxAveragePitch()
            {
                _pitchContainer.Add(pitch);
                if (_pitchContainer.Count >= _pitchRecordTime)
                {
                    _pitchContainer.RemoveAt(0);
                }

                float minPitch = float.MaxValue;
                float maxPitch = float.MinValue;
                float avgPitch = 0;

                foreach (var pit in _pitchContainer)
                {
                    if (pit < minPitch)
                    {
                        minPitch = pit;
                    }
                    else if (pit > maxPitch)
                    {
                        maxPitch = pit;
                    }

                    avgPitch += pit;
                }

                this.minPitch = minPitch;
                this.maxPitch = maxPitch;
                this.avgPitch = avgPitch / _pitchContainer.Count;
            }
        }

        float HighPassFilter(float pitch, float cutOff)
        {
            if (pitch > cutOff)
            {
                return 0;
            }
            else
            {
                return pitch;
            }
        }

    }


    interface VolumeProvider
    {
        float volume { get; set; }
        float pitch { get; set; }
        float avgVolume { get; set; }
        float avgPitch { get; set; }
        float maxVolume { get; set; }
        float minVolume { get; set; }
        float maxPitch { get; set; }
        float minPitch { get; set; }
    }
}