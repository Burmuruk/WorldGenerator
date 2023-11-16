using System.Collections.Generic;
using WorldG.Patrol;

namespace Coco.AI.PathFinding
{
    public interface IPathFinder
	{
        LinkedList<IPathNode> Get_Route(IPathNode start, IPathNode end, out float distance);

        LinkedList<IPathNode> Find_Route(IPathNode start, IPathNode end, out float distance);

        void SetNodeList(IPathNode[] nodes);
    } 
}
