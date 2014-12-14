using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBoardLEDs
{
    public class MappedProject
    {
        private Project m_Project;
        private BoardLocation m_Location;

        public MappedProject(Project project, BoardLocation location)
        {
            m_Project = project;
            m_Location = location;
        }

        public Project Project
        {
            get { return m_Project; }
        }

        public BoardLocation Location
        {
            get { return m_Location; }
        }
    }
}
