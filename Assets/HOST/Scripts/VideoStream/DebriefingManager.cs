//using FfmpegUnity;
using FfmpegUnity;
using HOST.Monitoring;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HOST.Debriefing
{
    public class DebriefingManager : MonoBehaviour
    {
        private string _directoryPath;
        private string simulationsDirectoryPath = Application.dataPath + "/Simulations/";
        private string _srtFileName = "comments.srt";

        public bool IsRecording = false;

        public RenderTexture VideoRenderTexture;

        public GameObject MainVideoViewport;

        public FfmpegCaptureCommand CaptureVideo;

        public FfmpegCommand subtitlesBurner;

        [SerializeField]
        private PDFReportGenerator pdfReportGenerator;

        private string filename;
        private List<Comment> comments = new List<Comment>();
        public struct Comment
        {
            public string text;
            public TimeSpan time;
            public byte[] image;
        }


        public void StartRecordingVideo()
        {
            CaptureVideo_Windows(FindAnyObjectByType<ScenarioManager>().settings.SceneName);

            IsRecording = true;
        }

        private void CaptureVideo_Windows(string scenario)
        {
            try
            {
                filename = string.Format("Scenario_{0}_{1:yyyy-MM-dd_HH-mm}", scenario, DateTime.Now);
                _directoryPath = simulationsDirectoryPath + "Scenario_" + scenario + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm") + "/";

                Debug.Log("Saving video to: " + _directoryPath);
                Debug.Log("Saving video as: " + filename);

                // Create the path if it doesn't exist
                DirectoryInfo di = Directory.CreateDirectory(_directoryPath);

                // Add parameters to CaptureVideo tool (where to save the video) and start it
                CaptureVideo.CaptureOptions += $"\n\"{_directoryPath + filename + ".mp4"}\"";

                CaptureVideo.StartFfmpeg();

                // Generate the subtitle file and add a default message at the start
            }
            catch (Exception e)
            {
                Debug.LogError($"Could not start video capture" +
                    $"\n{e.Message}" +
                    $"\n\n{e.StackTrace}\n");
            }

        }

        public string GetSurroundingTimeStamp(TimeSpan time, float offset, float duration)
        {
            TimeSpan startTime = time + TimeSpan.FromSeconds(offset);
            TimeSpan endTime = startTime + TimeSpan.FromSeconds(duration);
            return startTime.ToString(@"hh\:mm\:ss\,fff") + " --> " + endTime.ToString(@"hh\:mm\:ss\,fff");
        }

        public void AddComment(TMP_Text commentInput)
        {
            var image = MainVideoViewport.GetComponentInChildren<RawImage>();
            Texture2D texture = null;
            if (image.texture is Texture2D)
            {
                texture = image.texture as Texture2D;
            }
            // Webcam texture needs to be converted to encode as png in the database
            else if (image.texture is WebCamTexture)
            {
                WebCamTexture tex = image.texture as WebCamTexture;
                texture = new Texture2D(tex.width, tex.height);
                texture.SetPixels(tex.GetPixels());
                texture.Apply();
            }

            Comment comment = new Comment()
            {
                text = commentInput.text,
                time = TimeSpan.FromSeconds(ScenarioManager.instance.GetTime()),
                image = texture.EncodeToPNG(),
            };

            comments.Add(comment);
            commentInput.text = "";

        }

        public void WriteCommentsAsSubtitle()
        {
            int commentIndex = 1;
            StreamWriter srtWriter = new StreamWriter(_directoryPath + _srtFileName);
            foreach (Comment c in comments)
            {
                srtWriter.WriteLine(commentIndex.ToString());
                srtWriter.WriteLine(GetSurroundingTimeStamp(c.time, -5f, 5f));
                srtWriter.WriteLine(c.text);
                srtWriter.WriteLine();
                commentIndex++;
            }

            srtWriter.Close();
        }

        public void StopRecordingVideo()
        {
            CaptureVideo.StopFfmpeg();

            IsRecording = false;


            if(comments.Count > 0)
            {
                WriteCommentsAsSubtitle();
                pdfReportGenerator.CreateDocumentAsTask(comments, _directoryPath + filename + ".pdf");
                //subtitlesBurner.Options = string.Format("-i \"{0}\" -vf subtitles={1} \"{2}\"", _directoryPath + filename + ".mp4", _directoryPath+_srtFileName, _directoryPath + filename + "_subtitled.mp4");
                //subtitlesBurner.ExecuteFfmpeg();
            }
            
        }

        public void Update()
        {
            if (IsRecording)
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