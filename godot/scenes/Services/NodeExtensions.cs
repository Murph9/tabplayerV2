using System.Collections.Generic;
using Godot;

namespace murph9.TabPlayer.scenes.Services;

public static class NodeExtensions {

    public static void AddChildren(this Node node, IEnumerable<Node> nodes) {
        foreach (var n in nodes) {
            node.AddChild(n);
        }
    }
}