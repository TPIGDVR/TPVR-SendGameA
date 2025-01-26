using System;
using System.Collections.Generic;
using Experiement__Voice_Recognition_;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;


namespace NewBreathingDetector
{
    public class AnotherAnalysis : MonoBehaviour
    {
        public AudioSource audioSource; // Assign the AudioSource in the Inspector
        [SerializeField] int spectrumSize = 1024; // Number of samples for spectrum data (e.g., 1024 or 2048)
        public float heightMultiplier = 10f; // Multiplier to scale the visual representation

        //additional
        [SerializeField] private int bufferSize = 32768; // Size of the rolling buffer
        private float[] rollingBuffer; // Circular buffer to store audio data
        private int writeIndex;

        private float[] spectrumData;

        [SerializeField] private int recordingSize = 1024;
        private Queue<float> voltageOverTimeSize = new Queue<float>();

        [Header("CALCULATION")] [SerializeField]
        CalculationMethod calculationMethod = CalculationMethod.INCLUDENEGATIVE;

        enum CalculationMethod
        {
            INCLUDENEGATIVE = 0,
            ONLYPOSITIVE,
        }

        [Header("DownSampling")] [SerializeField]
        bool enableDownSampling = true;

        [SerializeField] private int downScalingFactor = 100;
        [SerializeField] DownSamplingType downSamplingType = DownSamplingType.UNIFORM;

        enum DownSamplingType
        {
            UNIFORM,
            AVERAGE,
            PEAK,
        }

        // Filter coefficients for the Butterworth filter
        private float[] a = new float[4];
        private float[] b = new float[4];
        private float[] x = new float[4];
        private float[] y = new float[4];

        [Header("Butterworth low pass filter")] [SerializeField]
        bool enableButterworthLowPass = true;

        [SerializeField] float cutOffFrequency = 0f;
        private FilterButterworth butterworthFilter;

        private void Start()
        {
            if (audioSource == null)
            {
                Debug.LogError("Please assign an AudioSource component in the inspector.");
                return;
            }

            spectrumData = new float[spectrumSize];
            rollingBuffer = new float[bufferSize];

            butterworthFilter = new(cutOffFrequency,
                (int)(48000 / downScalingFactor),
                FilterButterworth.PassType.Lowpass,
                math.sqrt(2));

            InitializeButterworthFilter(cutOffFrequency, 48000.0f); // Adjust cutoff and sampling frequency as needed
        }

        private void Update()
        {
            // GenerateBreathingData();
            Maxing();
        }

        private void Maxing()
        {
            audioSource.GetOutputData(spectrumData, 0);
            ShiftBuffer(spectrumData);

            // UpdateRollingBuffer();

            //downsampling the data.
            var targetBuffer = rollingBuffer;
            if (enableDownSampling)
            {
                switch (downSamplingType)
                {
                    case DownSamplingType.UNIFORM:
                        targetBuffer = DownsampleUniform(rollingBuffer, downScalingFactor);
                        break;
                    case DownSamplingType.AVERAGE:
                        targetBuffer = DownsampleAverage(rollingBuffer, downScalingFactor);
                        break;
                    case DownSamplingType.PEAK:
                        targetBuffer = DownsamplePeaks(rollingBuffer, downScalingFactor);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (enableButterworthLowPass)
            {
                // float sampleRate = 1 / (48000f / (downScalingFactor / 2.15f));

                //For now, create a runtime butterworth filter to test the values
                var rtButterworthFilter = new FilterButterworth(cutOffFrequency,
                    (int)(48000 / downScalingFactor),
                    FilterButterworth.PassType.Lowpass,
                    math.sqrt(2));
                for (int i = 0; i < targetBuffer.Length; i++)
                {
                    butterworthFilter.Update(rollingBuffer[i] * rollingBuffer[i]);
                    targetBuffer[i] = math.sqrt(butterworthFilter.Value);
                }

                // ApplyButterworthFilter(targetBuffer);
                print("finish applying filter");
            }


            //for seeing the end result.
            print($"buffer size {targetBuffer.Length}");
            PlotAudioData(targetBuffer);
        }

        private void UpdateRollingBuffer()
        {
            // Write the new chunk into the rolling buffer
            for (int i = 0; i < spectrumSize; i++)
            {
                rollingBuffer[(writeIndex + i) % bufferSize] = spectrumData[i];
            }

            // Update the write position
            writeIndex = (writeIndex + spectrumSize) % bufferSize;
        }

        [ContextMenu("Generate Breathing")]
        private void GenerateBreathingData()
        {
            audioSource.GetOutputData(spectrumData, 0);
            float max = 0;
            bool isNegative = false;
            foreach (float sample in spectrumData)
            {
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
                        if (sample < 0) isNegative = true;
                        max = absVal;
                    }
                }

                void OnlyPositive()
                {
                    float result = (sample + 1f) / 2f;
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

        void ShiftBuffer(float[] newChunk)
        {
            // Shift the existing buffer to make room for the new chunk
            int shiftAmount = Mathf.Min(bufferSize - spectrumSize, bufferSize);
            System.Array.Copy(rollingBuffer, 0, rollingBuffer, spectrumSize, shiftAmount);

            // Copy the new chunk to the start of the buffer
            System.Array.Copy(newChunk, 0, rollingBuffer, 0, spectrumSize);
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

        #region butterworth filter

        private void InitializeButterworthFilter(float cutoffFrequency, float samplingFrequency)
        {
            float wc = 2.0f * Mathf.PI * cutoffFrequency / samplingFrequency; // Normalized cutoff frequency
            float wc2 = wc * wc;
            float wc3 = wc2 * wc;

            float sqrt2 = Mathf.Sqrt(2.0f);

            float denom = (1 + sqrt2 * wc + wc2) * (1 + wc);
            b[0] = wc3 / denom;
            b[1] = 3 * b[0];
            b[2] = 3 * b[0];
            b[3] = b[0];

            a[0] = 1.0f; // Not used in the difference equation but included for consistency
            a[1] = (3 + 2 * wc + wc2 - 3 * sqrt2 * wc) / denom;
            a[2] = (3 - 2 * wc + wc2) / denom;
            a[3] = (1 - sqrt2 * wc + wc2) / denom;
        }

        private void ApplyButterworthFilter(float[] data)
        {
            // float[] filteredData = new float[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                //square the data first
                float curData = data[i] * data[i];

                // Shift input history
                x[3] = x[2];
                x[2] = x[1];
                x[1] = x[0];
                x[0] = curData;

                // Calculate filtered output
                y[3] = y[2];
                y[2] = y[1];
                y[1] = y[0];
                y[0] = b[0] * x[0] + b[1] * x[1] + b[2] * x[2] + b[3] * x[3]
                       - a[1] * y[1] - a[2] * y[2] - a[3] * y[3];

                data[i] = math.sqrt(y[0]);
            }
        }

        #endregion

        // void PlotAudioData(float[] samples)
        // {
        //     // Loop through each sample and plot it as a line
        //     for (int i = 0; i < samples.Length - 1; i++)
        //     {
        //         // Calculate the start and end points for the line
        //         Vector3 startPoint = new Vector3(i * 0.1f, samples[i] * heightMultiplier, 0);
        //         Vector3 endPoint = new Vector3((i + 1) * 0.1f, samples[i + 1] * heightMultiplier, 0);
        //
        //         // Draw a line between the start and end points
        //         Debug.DrawLine(startPoint, endPoint, Color.green);
        //     }
        // }

        void PlotAudioData(float[] samples, int startingIndex)
        {
            // Loop through each sample and plot it as a line
            for (int i = 0; i < samples.Length; i++)
            {
                int index = (startingIndex + i + 1) % bufferSize; // Start after writeIndex
                // Calculate the start and end points for the line
                Vector3 startPoint = new Vector3(i * 0.1f, samples[index] * heightMultiplier, 0);
                Vector3 endPoint = new Vector3((i + 1) * 0.1f, samples[index + 1] * heightMultiplier, 0);

                // Draw a line between the start and end points
                Debug.DrawLine(startPoint, endPoint, Color.green);
            }
            
        }

        //Ploting the data
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
            
            print($"Current Data {String.Join(" ,", samples)}");

        }

        private void OnDrawGizmos()
        {
            CreateYAxis();
            //draw Y axis
            void CreateYAxis()
            {
                Vector3 startPoint = new Vector3(0, -heightMultiplier, 0);
                Vector3 endPoint = new Vector3(0, heightMultiplier, 0);
                Debug.DrawLine(startPoint, endPoint, Color.green);

                
                for (int i = -(int)heightMultiplier; i < (int)heightMultiplier; i++)
                {
                    DrawText($"{i/heightMultiplier}" , new Vector3(-0.5f, i , 0), Color.green);    
                }
            }
        }

        // Helper method to display text labels in the Scene view
        void DrawText(string text, Vector3 position, Color color)
        {
            Handles.color = color;
            Handles.Label(position, text);
        }
    }
}