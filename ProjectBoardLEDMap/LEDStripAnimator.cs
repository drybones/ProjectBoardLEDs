using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectBoardLEDs
{
    public class LEDStripAnimator
    {
        private IEnumerable<MappedProject> mappedProjects;
        private OPC opc;

        public LEDStripAnimator(IEnumerable<MappedProject> mappedProjects, OPC opc)
        {
            this.mappedProjects = mappedProjects;
            this.opc = opc;
        }

        public void Animate()
        {
            foreach (var mappedProject in mappedProjects)
            {
                AnimateProject(mappedProject);
            }
        }

        private void AnimateProject(MappedProject mappedProject)
        {
            switch (mappedProject.Project.Status)
            {
                case ProjectStatus.Pass:
                    AnimateStaticRegion(mappedProject.Location, 0, 128, 0);
                    break;
                case ProjectStatus.Fail:
                    AnimateStaticRegion(mappedProject.Location, 128, 0, 0);
                    break;
                case ProjectStatus.Slow:
                    AnimateSineWave(mappedProject.Location, 172, 150, 0, 1.0, 0.5, 2, 0.25);
                    break;
                case ProjectStatus.RedAlert:
                    AnimatePulsingRegion(mappedProject.Location, 255, 0, 0, 0.8, 0.4, 0.5);
                    break;
                case ProjectStatus.InProgress:
                    AnimateMovingGradient(mappedProject.Location, 0, 0, 172, 1.0, 0.4, 0.33, 0.85);
                    break;
            }
        }

        private void AnimateStaticRegion(BoardLocation location, byte r, byte g, byte b)
        {
            for (var i = location.FirstLED; i <= location.LastLED; i++)
            {
                SetPixel(location.StripNumber, i, r, g, b);
            }
        }

        private void AnimatePulsingRegion(BoardLocation location, byte r, byte g, byte b, double high, double low, double frequency)
        {
            double brightness = ((1.0 - Math.Cos(Environment.TickCount / 1000.0 * Math.PI * 2.0 * frequency)) / 2.0 * (high - low) + low);
            r = Convert.ToByte(brightness * r);
            g = Convert.ToByte(brightness * g);
            b = Convert.ToByte(brightness * b);

            for (var i = location.FirstLED; i <= location.LastLED; i++)
            {
                SetPixel(location.StripNumber, i, r, g, b);
            }
        }

        private void AnimateMovingGradient(BoardLocation location, byte r, byte g, byte b, double high, double low, double frequency, double rampLength = 1.0)
        {
            var regionLength = location.LastLED - location.FirstLED + 1;
            double movingOffset = (Environment.TickCount / 1000.0 * frequency) % 1.0;

            for (var i = location.FirstLED; i <= location.LastLED; i++)
            {
                var normalisedPosition = 1.0 * (i - location.FirstLED) / regionLength;
                var movingPosition = ((normalisedPosition - movingOffset + 1.0) % 1.0);
                var brightness = (movingPosition <= rampLength) ? (movingPosition / rampLength) * (high - low) + low : (1.0 - movingPosition) / (1.0 - rampLength) * (high - low) + low;
                SetPixel(location.StripNumber, i, Convert.ToByte(brightness * r), Convert.ToByte(brightness * g), Convert.ToByte(brightness * b));
            }
        }

        private void AnimateSineWave(BoardLocation location, byte r, byte g, byte b, double high, double low,
            double periods, double frequency)
        {
            var regionLength = location.LastLED - location.FirstLED + 1;
            double movingOffset = (Environment.TickCount / 1000.0 * frequency) % 1.0;
            for (var i = location.FirstLED; i <= location.LastLED; i++)
            {
                var normalisedPosition = 1.0 * (i - location.FirstLED) / regionLength;
                var movingPosition = ((normalisedPosition - movingOffset + 1.0) % 1.0);
                var brightness = 0.5 * (Math.Sin(movingPosition * periods * 2.0 * Math.PI) + 1.0) * (high - low) + low;
                SetPixel(location.StripNumber, i, Convert.ToByte(brightness * r), Convert.ToByte(brightness * g), Convert.ToByte(brightness * b));
            }

        }

        private void SetPixel(int stripNumber, int i, byte r, byte g, byte b)
        {
            opc.setPixel(stripNumber * 64 + i, r, g, b);
        }
    }
}
