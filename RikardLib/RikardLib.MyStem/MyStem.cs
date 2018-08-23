using System;
using System.Collections.Generic;
using System.Text;
using RikardLib.Log;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace RikardLib.MyStem
{
    public class MyStem
    {
        private readonly Logger logger = new Logger();

        private string MyStemExe
        {
            get
            {
                return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "mystem.exe" : "mystem";
            }
        }

        private readonly StringBuilder MyStemBuffer = new StringBuilder();

        public async Task<List<LxWord>> Stem(List<string> words)
        {
            using (var p = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    RedirectStandardError = true,
                    StandardErrorEncoding = Encoding.UTF8,
                    RedirectStandardInput = true,
                    FileName = MyStemExe,
                    Arguments = "--format json -ing --eng-gr --weight"
                }
            })
            {
                var lxWords = new List<LxWord>();

                try
                {
                    p.OutputDataReceived += OutputDataReceivedEvent;
                    p.ErrorDataReceived += ErrorDataReceivedEvent;

                    p.Start();
                    p.BeginOutputReadLine();
                    p.BeginErrorReadLine();

                    using (StreamWriter utf8Writer = new StreamWriter(p.StandardInput.BaseStream, new UTF8Encoding(false)))
                    {
                        utf8Writer.AutoFlush = true;

                        foreach (var w in words)
                        {
                            await utf8Writer.WriteLineAsync(w);
                        }
                    }

                    p.WaitForExit();

                    string rawOutput = MyStemBuffer.ToString();

                    lxWords.AddRange(JsonConvert.DeserializeObject<List<LxWord>>("[" + rawOutput.Replace('\n', ',') + "]"));

                    if(lxWords.Count != words.Count)
                    {
                        logger.Warn($"Perhaps something went wrong. MyStem had consumed {words.Count} words, but it have thrown out {lxWords.Count}.");
                    }
                }
                catch (Exception e)
                {
                    logger.Info($"While executing process.", e);
                }

                return lxWords;
            }
        }

        private void ErrorDataReceivedEvent(object sender, DataReceivedEventArgs e)
        {
            
        }

        private void OutputDataReceivedEvent(object sender, DataReceivedEventArgs e)
        {
            if (!String.IsNullOrEmpty(e.Data))
            {
                MyStemBuffer.AppendLine(e.Data);
            }
        }

        public bool CheckMyStemOnPath()
        {
            var p = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    FileName = MyStemExe,
                    Arguments = "-?"
                }
            };

            try
            {
                p.Start();
                p.WaitForExit();
                return true;
            }
            catch(Exception e)
            {
                logger.Info($"While executing process.", e);
                return false;
            }
        }
    }
}
