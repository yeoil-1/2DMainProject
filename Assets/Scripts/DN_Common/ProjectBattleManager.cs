using System.Collections.Generic;
using UnityEngine;

public class ProjectBattleManager : MonoBehaviour
{
    public static ProjectBattleManager Inst { get; set; }

    [SerializeField] private Project_BattlePlayer Player_Main;

    private void Awake()
    {
        Inst = this;
    }

    private void Start()
    {

        if (Player_Main != null)
        {
            DaniTechGameObjectManager.Inst.RegisterBattlePlayer(Player_Main);
            Player_Main.InitBattlePlayer("CH_001", 100);
        }

        DaniTechGameObjectManager.Inst.RequestSpawnEnemy();
    }

    public void RequestPlayCard(ProjectCardModel cardModel, int targetInstanceId, bool isUpgradedCard = false)
    {
        ProjectCardManager.Inst.ExecuteCardEffect(cardModel, targetInstanceId, isUpgradedCard);
    }
}