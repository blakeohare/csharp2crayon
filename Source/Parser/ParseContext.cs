using CSharp2Crayon.Parser.Nodes;
using System;
using System.Collections.Generic;

namespace CSharp2Crayon.Parser
{
    public class ParserContext
    {
        private List<TopLevelEntity> topLevelEntities = new List<TopLevelEntity>();

        public void AddEntity(TopLevelEntity tle)
        {
            this.topLevelEntities.Add(tle);
        }
    }
}
