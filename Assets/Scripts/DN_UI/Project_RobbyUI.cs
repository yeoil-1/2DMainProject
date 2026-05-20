using UnityEngine;

public class Project_RobbyUI : UIBase
{
    [SerializeField] private DaniTechUIButton Button_Continue;
    [SerializeField] private DaniTechUIButton Button_GameStart;
    [SerializeField] private DaniTechUIButton Button_GiveUp;
    [SerializeField] private DaniTechUIButton Button_MultyPlay;
    [SerializeField] private DaniTechUIButton Button_TimeLine;
    [SerializeField] private DaniTechUIButton Button_Settings;
    [SerializeField] private DaniTechUIButton Button_Encyclopedia;
    [SerializeField] private DaniTechUIButton Button_GameQuit;
    
    private void OnEnable()
    {
        Button_GameStart.BindOnClickButtonEvent(OnClick_GameStart);
        Button_GameQuit.BindOnClickButtonEvent(OnClick_GameQuit);
        Button_GiveUp.BindOnClickButtonEvent(OnClick_GiveUp);
        Button_MultyPlay.BindOnClickButtonEvent(OnClick_MultyPlay);
        Button_TimeLine.BindOnClickButtonEvent(OnClick_TimeLine);
        Button_Settings.BindOnClickButtonEvent(OnClick_Settings);
        Button_Encyclopedia.BindOnClickButtonEvent(OnClick_Encyclopedia);

    }

    public void OnClick_GameStart()
    {

    }

    public void OnClick_GameQuit()
    {

    }

    public void OnClick_GiveUp()
    {

    }

    public void OnClick_MultyPlay()
    {

    }

    public void OnClick_TimeLine()
    {

    }

    public void OnClick_Settings()
    {

    }

    public void OnClick_Encyclopedia()
    {

    }
}
