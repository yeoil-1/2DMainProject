using System.Collections;
using UnityEngine;

public enum ProjectTurnState
{
    None = 0,
    PlayerTurn,
    EnemyTurn
}

public class DaniTechTurnManager : MonoBehaviour
{
    public static DaniTechTurnManager Inst { get; set; }

    private ProjectTurnState _currentTurnState = ProjectTurnState.None;
    private int _turnCount = 0;

    public ProjectTurnState CurrentTurnState
    {
        get => _currentTurnState;
        private set => _currentTurnState = value;
    }

    private void Awake()
    {
        Inst = this;
    }

    public void StartBattle()
    {
        _turnCount = 0;
        Debug.Log("전투가 시작되었습니다.");


        ChangeTurn(ProjectTurnState.PlayerTurn);
    }


    public void ChangeTurn(ProjectTurnState nextTurn)
    {
        CurrentTurnState = nextTurn;

        if (CurrentTurnState == ProjectTurnState.PlayerTurn)
        {
            _turnCount++;
            StartPlayerTurn();
        }
        else if (CurrentTurnState == ProjectTurnState.EnemyTurn)
        {
            StartEnemyTurn();
        }
    }

    private void StartPlayerTurn()
    {
        Debug.Log($"<color=green>[턴 {_turnCount}] 플레이어 턴 시작</color>");

        var player = DaniTechGameObjectManager.Inst.BattlePlayerTarget;
        if (player != null)
        {
            player.ResetEnergyOnTurnStart();

            player.ProcessTurnStartEffects();
        }

        ProjectCardManager.Inst.DrawCards(5);

        EnterPlayerActionPhase();
    }

    private void EnterPlayerActionPhase()
    {
        Debug.Log("플레이어 행동 대기 중... 카드를 사용하거나 턴 종료 버튼을 누르세요.");
        // UI 매니저를 통해 '턴 종료 버튼'을 활성화하는 등의 연동 진행
    }

    // 유저가 화면의 [턴 종료] 버튼을 눌렀을 때 웅변적으로 호출될 public 함수
    public void RequestEndPlayerTurn()
    {
        // 검증: 플레이어 턴이 아닐 때는 무시
        if (CurrentTurnState != ProjectTurnState.PlayerTurn) return;

        Debug.Log("플레이어 턴 종료 요청 접수.");

        // 1. 플레이어 턴 종료 시점의 버프/디버프 소모 연산
        var player = DaniTechGameObjectManager.Inst.BattlePlayerTarget;
        if (player != null)
        {
            player.ProcessTurnEndEffects();
        }

        // 2. 사용하지 않고 남은 핸드 카드 처리 로직
        // CardManager.Inst.ClearRemainingHand();

        ChangeTurn(ProjectTurnState.EnemyTurn);
    }


    private void StartEnemyTurn()
    {
        Debug.Log("<color=red>적 턴 시작</color>");

        // 적들의 턴 시작 버프/디버프 처리 (몬스터 객체들도 순회하며 처리)
        // GameObjectManager.Inst.ProcessEnemyTurnStartEffects();

        StartCoroutine(Co_ExecuteEnemyAI());
    }

    private IEnumerator Co_ExecuteEnemyAI()
    {

        Debug.Log("적이 플레이어를 공격합니다!");

        // GameObjectManager를 통해 등록된 플레이어(1번)에게 대미지 가하기
        var player = DaniTechGameObjectManager.Inst.BattlePlayerTarget;
        if (player != null)
        {
            //player.TakeDamage(); 
        }

        yield return new WaitForSeconds(1.0f);

        ChangeTurn(ProjectTurnState.PlayerTurn);
    }
}