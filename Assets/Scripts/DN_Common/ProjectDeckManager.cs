using NUnit.Framework;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class ProjectDeckManager : MonoBehaviour
{
    public static ProjectDeckManager Inst { get; set; }

    private void Awake()
    {
        Inst = this;
    }

    public List<ProjectCardModel> GetCurrentBattleDeck()
    {
        var allCards = DaniTechGameManager.Inst.GetPlayerCardList();
        var battleDeck = new List<ProjectCardModel>();

        foreach (var card in allCards)
        {
            if (card.IsInDeck)
            {
                battleDeck.Add(card);
            }
        }
        return battleDeck;
    }

}
