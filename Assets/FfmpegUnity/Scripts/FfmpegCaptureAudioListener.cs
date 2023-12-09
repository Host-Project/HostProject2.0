using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FfmpegUnity
{
    public class FfmpegCaptureAudioListener : MonoBehaviour
    {
        public int StreamId
        {
            set;
            get;
        }

        public int Channels
        {
            set;
            get;
        }

        public int ReadCount
        {
            set;
            get;
        }

        void OnAudioFilterRead(float[] data, int channels)
        {
            ReadCount += data.Length / channels;
        }

        public float[] Read()
        {
            if (ReadCount <= 0)
            {
                return new float[0];
            }

            int readCount = 1;
            while (ReadCount >= readCount)
            {
                readCount *= 2;
            }
            readCount /= 2;

            float[] allSamples = new float[readCount * Channels];
            for (int loop = 0; loop < Channels; loop++)
            {
                float[] samples = new float[readCount];
                AudioListener.GetOutputData(samples, loop);
                for (int loop2 = 0; loop2 < readCount; loop2++)
                {
                    allSamples[loop2 * Channels + loop] = samples[loop2];
                }
            }

            ReadCount = 0;

            return allSamples;
        }
    }
}
