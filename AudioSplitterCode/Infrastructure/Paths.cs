using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioSplitter.Infrastructure
{
    static class Paths
    {
        public static string FfmpegExePath =>
           Path.Combine(AppContext.BaseDirectory, "ffmpeg", "win-x64", "ffmpeg.exe");
    }
}
