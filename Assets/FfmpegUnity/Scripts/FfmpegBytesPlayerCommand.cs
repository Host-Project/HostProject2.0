using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace FfmpegUnity
{
    public class FfmpegBytesPlayerCommand : FfmpegPlayerCommand, FfmpegBytesInputs.IInputControl
    {
        public string[] InputByteOptions = new string[1];

        FfmpegBytesInputs bytesInputs_;
        List<byte>[] tempBytes_;

        public bool InputBytesIsEmpty
        {
            get
            {
                if (bytesInputs_ == null)
                {
                    return true;
                }
                return bytesInputs_.IsEmpty;
            }
        }

        public void AddInputBytes(byte[] bytes, int inputNo = 0)
        {
            StartCoroutine(addInputBytes(bytes, inputNo));
        }

        IEnumerator addInputBytes(byte[] bytes, int inputNo = 0)
        {
            while (bytesInputs_ == null && tempBytes_ == null)
            {
                yield return null;
            }

            if (bytesInputs_ == null)
            {
                tempBytes_[inputNo].AddRange(bytes);
            }
            else
            {
                bytesInputs_.AddInputBytes(bytes, inputNo);
            }
        }

        protected override void Build()
        {
            RunOptions = "";
            StartCoroutine(allCoroutine());
        }

        protected override void Clean()
        {
            if (bytesInputs_ != null)
            {
                bytesInputs_.Dispose();
            }

            base.Clean();
        }

        IEnumerator allCoroutine()
        {
            //RunOptions += " -re ";

            if (AutoStreamSettings)
            {
                tempBytes_ = new List<byte>[InputByteOptions.Length];
                for (int loop = 0; loop < tempBytes_.Length; loop++)
                {
                    tempBytes_[loop] = new List<byte>();
                }

                yield return null;
                for (int loop = 0; loop < tempBytes_.Length; loop++)
                {
                    while (tempBytes_[loop].Count <= 0)
                    {
                        yield return null;
                    }
                }

                bool restart;
                do
                {
                    Streams = new FfmpegStream[0];
                    for (int loop = 0; loop < tempBytes_.Length; loop++)
                    {
                        string path = Application.temporaryCachePath + "/FfmpegUnity_" + Guid.NewGuid().ToString();
                        File.WriteAllBytes(path, tempBytes_[loop].ToArray());
                        yield return FfprobeCoroutine(path);
                        File.Delete(path);
                    }

                    restart = true;
                    foreach (var stream in Streams)
                    {
                        if (stream.Width > 0 && stream.Height > 0)
                        {
                            restart = false;
                        }
                    }
                    if (restart)
                    {
                        yield return null;
                    }
                } while (restart);
            }

            bytesInputs_ = new FfmpegBytesInputs(InputByteOptions, this);
            RunOptions += bytesInputs_.BuildAndStart();

            if (AutoStreamSettings)
            {
                for (int loop = 0; loop < tempBytes_.Length; loop++)
                {
                    bytesInputs_.AddInputBytes(tempBytes_[loop].ToArray(), loop);
                }
            }

            yield return ResetCoroutine();
        }
    }
}
