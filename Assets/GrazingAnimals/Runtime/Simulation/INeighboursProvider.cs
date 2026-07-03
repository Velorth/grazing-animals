using System.Collections.Generic;

namespace GrazingAnimals
{
    public interface INeighboursProvider
    {
        void CollectNeighbours(Agent agent, float neighbourDistance, List<Agent> neighbours);
    }
}