using System.Collections.Generic;

namespace CastIt.Models.FFMpeg.Args
{
    public class FFmpegArgsBuilder
    {
        private readonly List<FFmpegInputArgs> _inputs = new List<FFmpegInputArgs>();
        private readonly List<FFmpegOutputArgs> _outputs = new List<FFmpegOutputArgs>();

        public FFmpegInputArgs AddInputFile(string fileName)
        {
            var input = new FFmpegInputArgs(fileName);

            _inputs.Add(input);

            return input;
        }

        public FFmpegInputArgs AddStdIn()
        {
            var input = new FFmpegInputArgs("-");

            _inputs.Add(input);

            return input;
        }

        public FFmpegOutputArgs AddOutputFile(string fileName)
        {
            var output = new FFmpegOutputArgs(fileName);

            _outputs.Add(output);

            return output;
        }

        public FFmpegOutputArgs AddStdOut()
        {
            var output = new FFmpegOutputArgs("-");

            _outputs.Add(output);

            return output;
        }

        public FFmpegOutputArgs AddOutputPipe()
        {
            var output = new FFmpegOutputArgs("-");

            _outputs.Add(output);

            return output;
        }

        public string GetArgs()
        {
            var args = new List<string>();

            foreach (var input in _inputs)
            {
                args.Add(input.GetArgs());
            }

            foreach (var output in _outputs)
            {
                args.Add(output.GetArgs());
            }

            return string.Join(" ", args);
        }
    }
}
