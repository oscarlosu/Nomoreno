using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts {
    public struct Clue {
        public string Statement { get; set; }
        public NPC Target { get; set; }
        public ClueIdentifier Identifier { get; set; }
        public NPCPart.NPCPartType NPCPartType { get; set; }
        public NPCPart.NPCPartDescription NPCDescription { get; set; }

        public Clue(string statement, NPC target, ClueIdentifier identifier, NPCPart.NPCPartType targetPart) {
            Statement = statement;
            Target = target;
            Identifier = identifier;
            NPCPartType = targetPart;
            switch (NPCPartType) {
                case NPCPart.NPCPartType.Hat:   NPCDescription = Target.Head.Description; break;
                case NPCPart.NPCPartType.Shirt: NPCDescription = Target.Torso.Description; break;
                case NPCPart.NPCPartType.Pants: NPCDescription = Target.Legs.Description; break;
                default: throw new Exception("NPCPart unidentifiable");
            }
        }

    }
}
