using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorldG.Patrol;

namespace WorldG.Control
{
    public class Citizen : Minion
    {
        public override void SetWork(object args)
        {
            int id = (int)args;
        }
    }
}
