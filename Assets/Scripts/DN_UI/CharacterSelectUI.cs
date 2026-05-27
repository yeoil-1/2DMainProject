using UnityEngine;

public class CharacterSelectUI : DaniTechUIBase
{
    [SerializeField] private DaniTechUIButton Button_Character_1;
    [SerializeField] private DaniTechUIButton Button_Character_2;
    [SerializeField] private DaniTechUIButton Button_Character_3;
    [SerializeField] private DaniTechUIButton Button_Character_4;
    [SerializeField] private DaniTechUIButton Button_Character_5;
    [SerializeField] private DaniTechUIButton Button_RandomCharacter;
    [SerializeField] private DaniTechUIButton Button_Back;
    [SerializeField] private DaniTechUIButton Button_Select;
    [SerializeField] private DaniTechUIButton Button_DifficultyUp;
    [SerializeField] private DaniTechUIButton Button_DifficultyDown;

    private void OnEnable()
    {
        Button_Character_1.BindOnClickButtonEvent(OnClick_Character_1);
        Button_Character_2.BindOnClickButtonEvent(OnClick_Character_2);
        Button_Character_3.BindOnClickButtonEvent(OnClick_Character_3);
        Button_Character_4.BindOnClickButtonEvent(OnClick_Character_4);
        Button_Character_5.BindOnClickButtonEvent(OnClick_Character_5);
        Button_RandomCharacter.BindOnClickButtonEvent(OnClick_RandomCharacter);
        Button_Back.BindOnClickButtonEvent(OnClick_Back);
        Button_Select.BindOnClickButtonEvent(OnClick_Select);
        Button_DifficultyUp.BindOnClickButtonEvent(OnClick_DifficultyUp);
        Button_DifficultyDown.BindOnClickButtonEvent(OnClick_DifficultyDown);

    }

    public void OnClick_Character_1()
    {

    }

    public void OnClick_Character_2()
    {

    }

    public void OnClick_Character_3()
    {

    }

    public void OnClick_Character_4()
    {

    }

    public void OnClick_Character_5()
    {

    }

    public void OnClick_RandomCharacter()
    {

    }

    public void OnClick_Back()
    {

    }

    public void OnClick_Select()
    {

    }

    public void OnClick_DifficultyUp()
    {

    }

    public void OnClick_DifficultyDown()
    {

    }

 





}
