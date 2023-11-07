using System;
using System.Collections.Generic;
using System.Numerics;

namespace Coco.AI.PathFinding
{
	public interface IPathFinder
	{
        LinkedList<ScrNode> Get_Route(ScrNode start, ScrNode end, out float distance);

        LinkedList<ScrNode> Find_Route(ScrNode start, ScrNode end, out float distance);

        void SetNodeList(List<ScrNode> nodes);
    } 
}
