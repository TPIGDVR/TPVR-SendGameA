using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace NewBreathingDetector
{
    public class BreathAnalysisWithAudioSource : MonoBehaviour
    {
        public AudioSource audioSource; // Assign the AudioSource in the Inspector
        [SerializeField] int spectrumSize = 1024; // Number of samples for spectrum data (e.g., 1024 or 2048)
        public int downsampleFactor = 1;
        public float heightMultiplier = 10f; // Multiplier to scale the visual representation

        private float[] spectrumData;

        [SerializeField] private int recordingSize = 1024;
        private Queue<float> voltageOverTimeSize = new Queue<float>();
        
        [Header("CALCULATION")]
        [SerializeField]CalculationMethod calculationMethod = CalculationMethod.INCLUDENEGATIVE;
        enum CalculationMethod
        {
            INCLUDENEGATIVE = 0,
            ONLYPOSITIVE,
        }
        
        [Header("DownSampling")]
        [SerializeField] bool enableDownSampling = true;

        [SerializeField] private int downScalingFactor = 100;
        [SerializeField] DownSamplingType downSamplingType = DownSamplingType.UNIFORM;
        enum DownSamplingType
        {
            UNIFORM,
            AVERAGE,
            PEAK,
        }
        private void Start()
        {
            if (audioSource == null)
            {
                Debug.LogError("Please assign an AudioSource component in the inspector.");
                return;
            }

            spectrumData = new float[spectrumSize];
        }

        private void Update()
        {
            // GenerateBreathingData();
            Maxing();
        }

        private void Maxing()
        {
            audioSource.GetOutputData(spectrumData, 0);
            PlotAudioData(spectrumData);
        }

        [ContextMenu("Generate Breathing")]
        private void GenerateBreathingData()
        {
            audioSource.GetOutputData(spectrumData, 0);
            float max = 0;
            bool isNegative = false;
            foreach (float sample in spectrumData) {

                switch (calculationMethod)
                {
                    case CalculationMethod.INCLUDENEGATIVE:
                        IncludeNegative();
                        break;
                    case CalculationMethod.ONLYPOSITIVE:
                        OnlyPositive();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                void IncludeNegative()
                {
                    float absVal = math.abs(sample);
                    if (absVal > max)
                    {
                        if(sample < 0) isNegative = true;
                        max = absVal;
                    }
                }

                void OnlyPositive()
                {
                    float result = (sample + 1f) /2f;
                    if (result > max) max = result;
                }
            }
            
            //if the value is negative then change the max to negative
            if (isNegative) max = -max;
            
            //Store the voltage to a queue for reading
            voltageOverTimeSize.Enqueue(max);
            while (voltageOverTimeSize.Count > recordingSize)
            {
                voltageOverTimeSize.Dequeue();
            }
            
            var targetArray = voltageOverTimeSize.ToArray();
            if (enableDownSampling)
            {
                switch (downSamplingType)
                {
                    case DownSamplingType.UNIFORM:
                        targetArray = DownsampleUniform(targetArray, downScalingFactor);
                        break;
                    case DownSamplingType.AVERAGE:
                        targetArray = DownsampleUniform(targetArray, downScalingFactor);
                        break;
                    case DownSamplingType.PEAK:
                        targetArray = DownsampleUniform(targetArray, downScalingFactor);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            
            print($"data size now is {targetArray.Length}");

            PlotAudioData(targetArray);
        }

        #region downsampling

        float[] DownsampleUniform(float[] originalArray, int factor)
        {
            if (factor <= 0)
                throw new ArgumentException("Downsampling factor must be greater than 0.");
    
            int newLength = originalArray.Length / factor;
            float[] downsampledArray = new float[newLength];
    
            for (int i = 0; i < newLength; i++)
            {
                downsampledArray[i] = originalArray[i * factor];
            }
    
            return downsampledArray;
        }

        float[] DownsampleAverage(float[] originalArray, int factor)
        {
            if (factor <= 0)
                throw new ArgumentException("Downsampling factor must be greater than 0.");
    
            int newLength = originalArray.Length / factor;
            float[] downsampledArray = new float[newLength];
    
            for (int i = 0; i < newLength; i++)
            {
                float sum = 0;
                for (int j = 0; j < factor; j++)
                {
                    sum += originalArray[i * factor + j];
                }
                downsampledArray[i] = sum / factor;
            }
    
            return downsampledArray;
        }
        
        float[] DownsamplePeaks(float[] originalArray, int factor)
        {
            if (factor <= 0)
                throw new ArgumentException("Downsampling factor must be greater than 0.");
    
            int newLength = (int)Math.Ceiling((float)originalArray.Length / factor);
            float[] downsampledArray = new float[newLength];
    
            for (int i = 0; i < newLength; i++)
            {
                int start = i * factor;
                int end = Math.Min(start + factor, originalArray.Length);
                float max = float.MinValue;
                float min = float.MaxValue;
        
                for (int j = start; j < end; j++)
                {
                    if (originalArray[j] > max) max = originalArray[j];
                    if (originalArray[j] < min) min = originalArray[j];
                }
        
                downsampledArray[i] = (max + min) / 2; // or just max, min, or both
            }
    
            return downsampledArray;
        }

        #endregion
        
        void PlotAudioData(float[] samples)
        {
            // Loop through each sample and plot it as a line
            for (int i = 0; i < samples.Length - 1; i++)
            {
                // Calculate the start and end points for the line
                Vector3 startPoint = new Vector3(i * 0.1f, samples[i] * heightMultiplier, 0);
                Vector3 endPoint = new Vector3((i + 1) * 0.1f, samples[i + 1] * heightMultiplier, 0);

                // Draw a line between the start and end points
                Debug.DrawLine(startPoint, endPoint, Color.green);
            }
        }
    }
}