using Fusion;
using UnityEngine;

// ����ڷκ��� �Է��� ���� -> ȣ��Ʈ(���� ����)���� �Է� ������ ���� -> ��� Ŭ���̾�Ʈ���� ����ȭ
// OnInput���� ����� Set()�� T�� managed�� �����ϰ� �־� struct ���
public struct NetworkInputData : INetworkInput
{
    public const byte MOUSEBUTTON0 = 1;
    public const byte MOUSEBUTTON1 = 2;

    public NetworkButtons buttons;
    public Vector3 direction;
}
