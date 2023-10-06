using Godot;

namespace murph9.TabPlayer.scenes.Services;

public class TweenHelper {

    public static void TweenPosition(Tween t, Control n, Vector2 position, float time) {
        t.TweenProperty(n, "position", position, time).SetTrans(Tween.TransitionType.Quad);
    }
    
    public static void TweenAddPosition(Tween t, Control n, Vector2 addedPosition, float time) {
        t.TweenProperty(n, "position", n.Position + addedPosition, time).SetTrans(Tween.TransitionType.Quad);
    }
}
