using Fusion;
using UnityEngine;

public class Ball : NetworkBehaviour
{
    [Networked] // Fusion���� ����ȭ �ڵ带 ������ �� ����ϴ� �Ӽ�
    private TickTimer life { get; set; } // Ÿ�̸�

    public void Init()
    {
        life = TickTimer.CreateFromSeconds(Runner, 5.0f);
    }

    public override void FixedUpdateNetwork()
    {
        if (life.Expired(Runner))
            Runner.Despawn(Object);
        else
            transform.position += 5 * transform.forward * Runner.DeltaTime; // Runner.DeltaTime : Tick ���� �ð�
    }
}
