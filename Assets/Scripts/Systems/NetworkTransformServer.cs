using Unity.Netcode.Components;

public class NetworkTransformServer : NetworkTransform
{
    protected override bool OnIsServerAuthoritative() => true;
}
