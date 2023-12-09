using FfmpegUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using static Meta.WitAi.Data.AudioEncoding;

public class SubtitlesBurner : FfmpegCommand
{

    private string videoPath;
    private string srtPath;

    public string SrtPath { get => srtPath; set => srtPath = value; }
    public string VideoPath { get => videoPath; set => videoPath = value; }

    protected override void Build()
    {
        RunOptions = string.Format("-i {0} -vf subtitles={1} {2}", VideoPath, SrtPath, VideoPath.Replace(".mp4", "_subtitled.mp4"));
    }
    
    
}
