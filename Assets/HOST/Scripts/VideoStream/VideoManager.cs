//using FfmpegUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace HOST.VideoStream
{
    public class VideoManager : MonoBehaviour
    {
        private string _directoryPath;
        private string _rawVideoName = "video.mp4";
        private string _srtFileName = "video.srt";

        public bool IsRecording = false;

        public RenderTexture VideoRenderTexture;

        public GameObject MainVideoViewport;

       // public FfmpegCaptureCommand CaptureVideo;

        private StreamWriter _srtWriter;
        private int _commentIndex;



        public void StartRecordingVideo(string scenario)
        {
            CaptureVideo_Windows(scenario);

            IsRecording = true;
        }

        private void CaptureVideo_Windows(string scenario)
        {
            try
            {
                _directoryPath = Application.dataPath + "/../Simulations/Scenario_" + scenario + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm") + "/";

                // Create the path if it doesn't exist
                DirectoryInfo di = Directory.CreateDirectory(_directoryPath);

                // Add parameters to CaptureVideo tool (where to save the video) and start it
               /* CaptureVideo.CaptureOptions += $"\n\"{_directoryPath + _rawVideoName}\"";

                Debug.Log(CaptureVideo.CaptureOptions);
                CaptureVideo.StartFfmpeg();*/

                // Generate the subtitle file and add a default message at the start
            }
            catch (Exception e)
            {
                Debug.LogError($"Could not start video capture" +
                    $"\n{e.Message}" +
                    $"\n\n{e.StackTrace}\n");
            }

            try
            {
                _commentIndex = 1;

                //Pass the filepath and filename to the StreamWriter Constructor
                _srtWriter = new StreamWriter(_directoryPath + _srtFileName);

                _srtWriter.WriteLine(_commentIndex.ToString());
                WriteTimeStamp(TimeSpan.Zero, 0, 5);
                string timeNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                _srtWriter.WriteLine($"Scenario {scenario} {timeNow}");

                _commentIndex++;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
        }

        public void WriteTimeStamp(TimeSpan time, float offset, float duration)
        {
            TimeSpan startTime = time + TimeSpan.FromSeconds(offset);
            TimeSpan endTime = startTime + TimeSpan.FromSeconds(duration);
            _srtWriter.WriteLine(startTime.ToString(@"hh\:mm\:ss\,fff") + " --> " + endTime.ToString(@"hh\:mm\:ss\,fff"));
        }

        /*public void AddCommentOnVideo(Comment comment)
        {
            // Add the comment to a subtitle file instead, that will then be burned into the video
            _srtWriter.WriteLine();
            _srtWriter.WriteLine(_commentIndex.ToString());
            WriteTimeStamp(TimeSpan.FromMilliseconds(comment.GetTimeInSimulation()), -5f, 5f);
            _srtWriter.WriteLine(comment.GetContent());

            _commentIndex++;
        }*/

        public void StopRecordingVideo()
        {
           // CaptureVideo.StopFfmpeg();
            _srtWriter.Close();

            IsRecording = false;
        }

        public void Update()
        {
            if(IsRecording)
            {
                // Get the current texture 2d in the main video slot
                RawImage source = MainVideoViewport.GetComponentInChildren<RawImage>();

                if (source.texture != null)
                {
                    Graphics.Blit(source.texture, VideoRenderTexture);
                }
            }
        }
    }
}