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

    [Networked] // 네트워크를 통해 상태를 동기화하기 위한 속성
    public bool spawnedProjectile { get; set; } // 제공되는 get, set사용. 별도의 setter 메소드는 로컬에서만 작동한다

    private ChangeDetector _changeDetector; // 변경사항 감지기

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

    // 서버가 호출하는 FixedUpdate
    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            // BasicSpawner.cs의 OnInput()으로 수집한 입력을 아바타에 처리
            data.direction.Normalize();
            _cc.Move(5 * data.direction * Runner.DeltaTime); // 실제 player의 이동 코드

            
            if (data.direction.sqrMagnitude > 0)
                _forward = data.direction;

            // 버튼이 눌렸다면 프리팹 스폰
            if (data.buttons.IsSet(NetworkInputData.MOUSEBUTTON0))
            {
                Runner.Spawn(_prefabBall, transform.position + _forward, Quaternion.LookRotation(_forward), Object.InputAuthority);
                spawnedProjectile = !spawnedProjectile;
            }

            
            if (HasStateAuthority && delay.ExpiredOrNotRunning(Runner))
            {
                if (data.buttons.IsSet(NetworkInputData.MOUSEBUTTON0)) // 스폰 빈도 제한. 버튼 누름이 감지될 때만 타이머 재설정.
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

    // 변경사항 감지기 사용을 위해 Spawned 메소드 오버라이드
    public override void Spawned()
    {
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
    }

    // 마지막 Render 이후로 색상 값의 변화가 클라이언트에서 감지되면 MeshRenderer 업데이트
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

    // 클라이언트가 호스트에게 RPC를 호출하고 호스트가 모든 클라이언트에게 전달하는 방식
    // 아래 함수는 호스트가 모든 클라이언트에게 메시지를 전달하는 기능
    // RpcSources.InputAuthority: 객체에 대한 입력 권한이 있는 클라이언트만이 메시지를 보내기 위함
    // RpcTargets.StateAuthority: 메시지가 호스트로 전송
    // HostMode.SourceIsHostPlayer: 호스트 플레이어가 메시지를 보낸다는 것을 의미
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_SendMessage(string message, RpcInfo info = default)
    {
        RPC_RelayMessage(message, info.Source);
    }

    // 클라이언트가 호스트에게 메시지를 보내는 기능
    // RpcSources.StateAuthority: 호스트(/서버)가 이 RPC를 보냄
    // RpcTargets.All: 모든 클라이언트가 이 RPC를 받음
    // HostMode.SourceIsServer: 호스트 애플리케이션의 서버 부분이 이 RPC를 전송
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
