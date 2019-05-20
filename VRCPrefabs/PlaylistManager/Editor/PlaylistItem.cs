using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRCPrefabs.PlaylistManager
{
    public class PlaylistItem
    {
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Path { get; set; }
        public string Contents { get; set; }
        public FileInfo File { get; set; }

        public bool isSelected { get; set; }
    }
}