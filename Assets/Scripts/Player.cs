using Fusion;
using TMPro;
using UnityEngine;

public class Player : NetworkBehaviour
{
    private NetworkCharacterController _cc;

    [SerializeField] 
    private Ball _prefabBall;
    [SerializeField] 
    private PhysxBall _prefabPhysxBall;
    private Vector3 _forward = Vector3.forward;

    [Networked] 
    private TickTimer delay { get; set; }

    [Networked] // ��Ʈ��ũ�� ���� ���¸� ����ȭ�ϱ� ���� �Ӽ�
    public bool spawnedProjectile { get; set; } // �����Ǵ� get, set���. ������ setter �޼ҵ�� ���ÿ����� �۵��Ѵ�

    private ChangeDetector _changeDetector; // ������� ������

    public Material _material;

    private TMP_Text _messages;

    private void Awake()
    {
        _cc = GetComponent<NetworkCharacterController>();
        _material = GetComponentInChildren<MeshRenderer>().material;
    }

    private void Update()
    {
        if (Object.HasInputAuthority && Input.GetKeyDown(KeyCode.R))
        {
            RPC_SendMessage("Hey Mate!");
        }
    }

    // ������ ȣ���ϴ� FixedUpdate
    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            // BasicSpawner.cs�� OnInput()���� ������ �Է��� �ƹ�Ÿ�� ó��
            data.direction.Normalize();
            _cc.Move(5 * data.direction * Runner.DeltaTime); // ���� player�� �̵� �ڵ�

            
            if (data.direction.sqrMagnitude > 0)
                _forward = data.direction;

            // ��ư�� ���ȴٸ� ������ ����
            if (data.buttons.IsSet(NetworkInputData.MOUSEBUTTON0))
            {
                Runner.Spawn(_prefabBall, transform.position + _forward, Quaternion.LookRotation(_forward), Object.InputAuthority);
                spawnedProjectile = !spawnedProjectile;
            }

            
            if (HasStateAuthority && delay.ExpiredOrNotRunning(Runner))
            {
                if (data.buttons.IsSet(NetworkInputData.MOUSEBUTTON0)) // ���� �� ����. ��ư ������ ������ ���� Ÿ�̸� �缳��.
                {
                    delay = TickTimer.CreateFromSeconds(Runner, 0.5f);
                    Runner.Spawn(_prefabBall, transform.position + _forward, Quaternion.LookRotation(_forward), Object.InputAuthority,
                      (runner, o) =>
                      {
                          // Initialize the Ball before synchronizing it
                          o.GetComponent<Ball>().Init();
                      });
                }
                else if (data.buttons.IsSet(NetworkInputData.MOUSEBUTTON1))
                {
                    delay = TickTimer.CreateFromSeconds(Runner, 0.5f);
                    Runner.Spawn(_prefabPhysxBall, transform.position + _forward, Quaternion.LookRotation(_forward), Object.InputAuthority,
                      (runner, o) =>
                      {
                          o.GetComponent<PhysxBall>().Init(10 * _forward);
                      });
                }
            }
        }
    }

    // ������� ������ ����� ���� Spawned �޼ҵ� �������̵�
    public override void Spawned()
    {
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
    }

    // ������ Render ���ķ� ���� ���� ��ȭ�� Ŭ���̾�Ʈ���� �����Ǹ� MeshRenderer ������Ʈ
    public override void Render()
    {
        foreach (var change in _changeDetector.DetectChanges(this))
        {
            switch (change)
            {
                case nameof(spawnedProjectile):
                    _material.color = Color.white;
                    break;
            }
        }

        _material.color = Color.Lerp(_material.color, Color.blue, Time.deltaTime);
    }

    // Ŭ���̾�Ʈ�� ȣ��Ʈ���� RPC�� ȣ���ϰ� ȣ��Ʈ�� ��� Ŭ���̾�Ʈ���� �����ϴ� ���
    // �Ʒ� �Լ��� ȣ��Ʈ�� ��� Ŭ���̾�Ʈ���� �޽����� �����ϴ� ���
    // RpcSources.InputAuthority: ��ü�� ���� �Է� ������ �ִ� Ŭ���̾�Ʈ���� �޽����� ������ ����
    // RpcTargets.StateAuthority: �޽����� ȣ��Ʈ�� ����
    // HostMode.SourceIsHostPlayer: ȣ��Ʈ �÷��̾ �޽����� �����ٴ� ���� �ǹ�
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_SendMessage(string message, RpcInfo info = default)
    {
        RPC_RelayMessage(message, info.Source);
    }

    // Ŭ���̾�Ʈ�� ȣ��Ʈ���� �޽����� ������ ���
    // RpcSources.StateAuthority: ȣ��Ʈ(/����)�� �� RPC�� ����
    // RpcTargets.All: ��� Ŭ���̾�Ʈ�� �� RPC�� ����
    // HostMode.SourceIsServer: ȣ��Ʈ ���ø����̼��� ���� �κ��� �� RPC�� ����
    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_RelayMessage(string message, PlayerRef messageSource)
    {
        if (_messages == null)
            _messages = FindFirstObjectByType<TMP_Text>();

        if (messageSource == Runner.LocalPlayer)
        {
            message = $"You said: {message}\n";
        }
        else
        {
            message = $"Some other player said: {message}\n";
        }

        _messages.text += message;
    }
}
