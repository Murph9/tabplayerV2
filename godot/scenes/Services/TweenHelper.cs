using Godot;

namespace murph9.TabPlayer.scenes.Services;

public class TweenHelper {

    private readonly SceneTree _sceneTree;
    private readonly Control _control;
    private readonly string _propName;
    private readonly Godot.Variant _initialProp;
    private readonly Godot.Variant _finalProp;

    public TweenHelper(SceneTree sceneTree, Control control, string propName, Godot.Variant initial, Godot.Variant final) {
        _sceneTree = sceneTree;
        _control = control;
        _propName = propName;
        _initialProp = initial;
        _finalProp = final;
    }

    public float Speed { get; set; } = 1;
    public Tween.TransitionType Type { get; set; } = Tween.TransitionType.Quad;

    public void ToFinal() =>
        _sceneTree.CreateTween()
            .TweenProperty(_control, _propName, _finalProp, Speed)
            .SetTrans(Type);

    public void ToInitial() =>
        _sceneTree.CreateTween()
            .TweenProperty(_control, _propName, _initialProp, Speed)
            .SetTrans(Type);
}
