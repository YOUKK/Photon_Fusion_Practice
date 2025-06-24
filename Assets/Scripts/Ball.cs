using Fusion;
using UnityEngine;

public class Ball : NetworkBehaviour
{
    [Networked] // Fusion에서 직렬화 코드를 생성할 때 사용하는 속성
    private TickTimer life { get; set; } // 타이머

    public void Init()
    {
        life = TickTimer.CreateFromSeconds(Runner, 5.0f);
    }

    public override void FixedUpdateNetwork()
    {
        if (life.Expired(Runner))
            Runner.Despawn(Object);
        else
            transform.position += 5 * transform.forward * Runner.DeltaTime; // Runner.DeltaTime : Tick 간의 시간
    }
}
