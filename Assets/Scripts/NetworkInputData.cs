using Fusion;
using UnityEngine;

// ����ڷκ��� �Է��� ���� -> ȣ��Ʈ(���� ����)���� �Է� ������ ���� -> ��� Ŭ���̾�Ʈ���� ����ȭ
// OnInput���� ����� Set()�� T�� managed�� �����ϰ� �־� struct ���
public struct NetworkInputData : INetworkInput
{
    public Vector3 direction;
}
