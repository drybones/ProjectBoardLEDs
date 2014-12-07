using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBoardLEDs
{
    public class LEDStripAnimator
    {
        private IEnumerable<MappedProject> mappedProjects;
        private byte[] data;
        public byte[] Data { get { return data; } }

        public LEDStripAnimator(IEnumerable<MappedProject> mappedProjects)
        {
            this.mappedProjects = mappedProjects;
            var numLEDs = mappedProjects.Max(mp => mp.LastLED) + 1; // lastLED is a position, not a count
            data = new byte[4 + 3 * numLEDs]; // 4 header bytes, RGB
            data[0] = data[1] = data[2] = data[3] = 0; // Header bytes
        }

        public void Animate()
        {
            foreach(var mappedProject in mappedProjects)
            {
                AnimateProject(mappedProject);
            }
        }

        private void AnimateProject(MappedProject mappedProject)
        {
            switch(mappedProject.Status)
            {
                case ProjectStatus.Pass:
                    AnimateStaticRegion(mappedProject.FirstLED, mappedProject.LastLED, 0, 128, 0);
                    break;
                case ProjectStatus.Slow:
                    AnimateStaticRegion(mappedProject.FirstLED, mappedProject.LastLED, 128, 100, 0);
                    break;
                case ProjectStatus.Fail:
                    AnimateStaticRegion(mappedProject.FirstLED, mappedProject.LastLED, 128, 0, 0);
                    break;
                case ProjectStatus.RedAlert:
                    AnimatePulsingRegion(mappedProject.FirstLED, mappedProject.LastLED, 255, 0, 0, 0.8, 0.2, 0.5);
                    break;
                case ProjectStatus.InProgress:
                    AnimateMovingGradient(mappedProject.FirstLED, mappedProject.LastLED, 0, 0, 128, 1.0);
                    break;
            }
        }

        private void AnimateStaticRegion(int firstLED, int lastLED, byte r, byte g, byte b)
        {
            for(var i=firstLED; i<=lastLED; i++)
            {
                SetPixel(i, r, g, b);
            }
        }

        private void AnimatePulsingRegion(int firstLED, int lastLED, byte r, byte g, byte b, double high, double low, double frequency)
        {
            double brightness = ((1.0 - Math.Cos(Environment.TickCount / 1000.0 * Math.PI * 2.0 * frequency)) / 2.0 * (high-low) + low);
            r = Convert.ToByte(brightness * r);
            g = Convert.ToByte(brightness * g);
            b = Convert.ToByte(brightness * b);

            for(var i=firstLED; i<=lastLED; i++)
            {
                SetPixel(i, r, g, b);
            }            
        }

        private void AnimateMovingGradient(int firstLED, int lastLED, byte r, byte g, byte b, double frequency)
        {
            var regionLength = lastLED - firstLED;
            double movingOffset = (Environment.TickCount / 1000.0 * frequency) % 1.0;

            for (var i = firstLED; i <= lastLED; i++)
            {
                var normalisedPosition = 1.0 * (i - firstLED ) / regionLength;
                var brightness = (normalisedPosition - movingOffset + 1.0) % 1.0;
                SetPixel(i, Convert.ToByte(brightness * r), Convert.ToByte(brightness * g), Convert.ToByte(brightness * b));
            }
        }

        private void SetPixel(int i, byte r, byte g, byte b)
        {
            data[4 + i*3 + 0] = r;
            data[4 + i*3 + 1] = g;
            data[4 + i*3 + 2] = b;
        }
    }
}
