// Game.Save/SaveSectionData.cs
using System;

namespace Game.Save
{
    [Serializable]
    public class SaveSectionData
    {
        public string key;
        public string json;
    }
}