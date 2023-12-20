using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ControlsMainMenu : UI
{
    public static ControlsMainMenu Instance { get; private set; }

    [SerializeField] private ButtonUI moveUpButton;
    [SerializeField] private ButtonUI moveDownButton;
    [SerializeField] private ButtonUI moveLeftButton; 
    [SerializeField] private ButtonUI moveRightButton;
    [SerializeField] private ButtonUI lightAttackButton;
    [SerializeField] private ButtonUI strongAttackButton;
    [SerializeField] private ButtonUI dashButton;
    [SerializeField] private ButtonUI interactButton;
    [SerializeField] private ButtonUI ability1Button;
    [SerializeField] private ButtonUI ability2Button;
    [SerializeField] private ButtonUI ability3Button;

    [SerializeField] private UI rebindUI;
    
    [SerializeField] private ButtonUI backButton;
    [SerializeField] private UI container;
    
    private IEnumerable<RectTransform> layoutGroupTransformsInChildren;
    
    private void Awake()
    {
       Instance = this; 
       Init();
       layoutGroupTransformsInChildren = GetComponentsInChildren<LayoutGroup>()
           .Select(x => x.GetComponent<RectTransform>());
    }

    private void Start()
    {
        moveUpButton.AddListener(() => {RebindBinding(GameInput.Bindings.MoveUp); });   
        moveDownButton.AddListener(() => {RebindBinding(GameInput.Bindings.MoveDown); });   
        moveLeftButton.AddListener(() => {RebindBinding(GameInput.Bindings.MoveLeft); });   
        moveRightButton.AddListener(() => {RebindBinding(GameInput.Bindings.MoveRight); });
       
        lightAttackButton.AddListener(() => {RebindBinding(GameInput.Bindings.LightAttack); });   
        strongAttackButton.AddListener(() => { RebindBinding(GameInput.Bindings.StrongAttack); });
        dashButton.AddListener(() => { RebindBinding(GameInput.Bindings.Dash); });
        interactButton.AddListener(() => { RebindBinding(GameInput.Bindings.Interact); });
       
        ability1Button.AddListener(() => {RebindBinding(GameInput.Bindings.Ability1); });
        ability2Button.AddListener(() => {RebindBinding(GameInput.Bindings.Ability2); });
        ability3Button.AddListener(() => {RebindBinding(GameInput.Bindings.Ability3); });

        UpdateVisual();
        
        backButton.AddListener(OnBackButtonClicked);
        
        container.gameObject.SetActive(false);
        rebindUI.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    private void ToggleButtons(bool enable)
    {
        if (enable)
        {
            moveUpButton.Enable();
            moveDownButton.Enable();
            moveLeftButton.Enable();
            moveRightButton.Enable();
            lightAttackButton.Enable();
            strongAttackButton.Enable();
            dashButton.Enable();
            interactButton.Enable();
            ability1Button.Enable();
            ability2Button.Enable();
            ability3Button.Enable();
            backButton.Enable();
        }
        else
        {
            moveUpButton.Disable();
            moveDownButton.Disable();
            moveLeftButton.Disable();
            moveRightButton.Disable();
            lightAttackButton.Disable();
            strongAttackButton.Disable();
            dashButton.Disable();
            interactButton.Disable();
            ability1Button.Disable();
            ability2Button.Disable();
            ability3Button.Disable();
            backButton.Disable();
        }
    }

    private void OnBackButtonClicked()
    {
        if(Hide())
            OptionsMainMenu.Instance.Show();
    }

    protected override void Activate()
    {
        ToggleButtons(true);
        foreach (var t in layoutGroupTransformsInChildren)
            LayoutRebuilder.ForceRebuildLayoutImmediate(t);
        container.Show();
    }

    protected override void Deactivate()
    {
        ToggleButtons(false);
        container.Hide();
    }

    private void UpdateVisual()
    {
        moveUpButton.buttonText.text = GameInput.Instance.GetBindingText(GameInput.Bindings.MoveUp);
        moveDownButton.buttonText.text = GameInput.Instance.GetBindingText(GameInput.Bindings.MoveDown);
        moveLeftButton.buttonText.text = GameInput.Instance.GetBindingText(GameInput.Bindings.MoveLeft);
        moveRightButton.buttonText.text = GameInput.Instance.GetBindingText(GameInput.Bindings.MoveRight);
        
        lightAttackButton.buttonText.text = GameInput.Instance.GetBindingText(GameInput.Bindings.LightAttack);
        strongAttackButton.buttonText.text = GameInput.Instance.GetBindingText(GameInput.Bindings.StrongAttack);
        dashButton.buttonText.text = GameInput.Instance.GetBindingText(GameInput.Bindings.Dash);
        interactButton.buttonText.text = GameInput.Instance.GetBindingText(GameInput.Bindings.Interact);
        
        ability1Button.buttonText.text = GameInput.Instance.GetBindingText(GameInput.Bindings.Ability1);
        ability2Button.buttonText.text = GameInput.Instance.GetBindingText(GameInput.Bindings.Ability2);
        ability3Button.buttonText.text = GameInput.Instance.GetBindingText(GameInput.Bindings.Ability3);

        foreach (var t in layoutGroupTransformsInChildren)
            LayoutRebuilder.ForceRebuildLayoutImmediate(t);
    }

    private void RebindBinding(GameInput.Bindings binding)
    {
        rebindUI.Show();
        Hide();
        GameInput.Instance.RebindBinding(binding, () =>
        {
            Show();
            rebindUI.Hide();
            UpdateVisual();
        }, () =>
        {
            WarningText.Instance.ShowPopup(2, "You cannot rebind a key to an already existing binding");
        });
    }
}