using Fusion;
using UnityEngine;

// 사용자로부터 입력을 수집 -> 호스트(서버 역할)에게 입력 데이터 전달 -> 모든 클라이언트에게 동기화
// OnInput에서 사용한 Set()이 T를 managed로 제한하고 있어 struct 사용
public struct NetworkInputData : INetworkInput
{
    public const byte MOUSEBUTTON0 = 1;
    public const byte MOUSEBUTTON1 = 2;

    public NetworkButtons buttons;
    public Vector3 direction;
}
