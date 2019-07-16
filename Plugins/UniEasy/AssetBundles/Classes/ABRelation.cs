using System.Collections.Generic;

namespace UniEasy
{
    public class ABRelation
    {
        private string abName;
        private List<string> dependenceABList;
        private List<string> referenceABList;

        public ABRelation(string abName)
        {
            if (!string.IsNullOrEmpty(abName))
            {
                this.abName = abName;
            }
            dependenceABList = new List<string>();
            referenceABList = new List<string>();
        }

        public void AddDependence(string abName)
        {
            if (!dependenceABList.Contains(abName))
            {
                dependenceABList.Add(abName);
            }
        }

        public bool RemoveDependece(string abName)
        {
            if (dependenceABList.Contains(abName))
            {
                dependenceABList.Remove(abName);
            }
            return !(dependenceABList.Count > 0);
        }

        public List<string> GetAllDependence()
        {
            return dependenceABList;
        }

        public void AddReference(string abName)
        {
            if (!referenceABList.Contains(abName))
            {
                referenceABList.Add(abName);
            }
        }

        public bool RemoveReference(string abName)
        {
            if (referenceABList.Contains(abName))
            {
                referenceABList.Remove(abName);
            }
            return !(referenceABList.Count > 0);
        }

        public List<string> GetAllReference()
        {
            return referenceABList;
        }
    }
}
