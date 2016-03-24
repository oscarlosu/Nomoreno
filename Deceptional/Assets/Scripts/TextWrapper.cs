using System.Text;
using UnityEngine;

namespace Assets.Scripts {
    public class TextWrapper : MonoBehaviour {
        public TextMesh Text;
        public int MaxLineCharacters;

        public void BreakLine() {
            StringBuilder sb = new StringBuilder(Text.text);

            if (Text.text.Length > MaxLineCharacters) {
                for (int i = 0; i < sb.Length; i++) {
                    if (i % MaxLineCharacters == 0) {
                        sb.Insert(i, "\n");
                    }
                }
            }
        }
    }
}
