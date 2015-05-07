using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientVisualisator.ForestObjects;

namespace ClientVisualisator.Visualisators
{
    public interface IForestVisualisator
    {
        void DrawForest(Forest forest);
        void AddInhabitantToDictionary(Inhabitant inhabitant);
    }
}
