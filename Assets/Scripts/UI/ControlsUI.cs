using System;
using UnityEngine;
using UnityEngine.UI;

public class ControlsUI : UI
{
    public static ControlsUI Instance { get; private set; }

    [SerializeField] private ButtonUI moveUpButton;
    [SerializeField] private ButtonUI moveDownButton;
    [SerializeField] private ButtonUI moveLeftButton; 
    [SerializeField] private ButtonUI moveRightButton;
    [SerializeField] private ButtonUI cameraLeftButton;
    [SerializeField] private ButtonUI cameraRightButton;
    [SerializeField] private ButtonUI lightAttackButton;
    [SerializeField] private ButtonUI strongAttackButton;
    [SerializeField] private ButtonUI dashButton;
    [SerializeField] private ButtonUI interactButton;
    [SerializeField] private ButtonUI ability1Button;
    [SerializeField] private ButtonUI ability2Button;
    [SerializeField] private ButtonUI ability3Button;
    
    [SerializeField] private Transform pressToRebindKeyTransform;
    
    [SerializeField] private ButtonUI backButton;
    [SerializeField] private UI container;
    
    private void Awake()
    {
       Instance = this; 
       Init();
    }

    private void Start()
    {
        moveUpButton.AddListener(() => {RebindBinding(GameInput.Bindings.MoveUp); });   
        moveDownButton.AddListener(() => {RebindBinding(GameInput.Bindings.MoveDown); });   
        moveLeftButton.AddListener(() => {RebindBinding(GameInput.Bindings.MoveLeft); });   
        moveRightButton.AddListener(() => {RebindBinding(GameInput.Bindings.MoveRight); });
       
        cameraLeftButton.AddListener(() => {RebindBinding(GameInput.Bindings.CameraLeft); });   
        cameraRightButton.AddListener(() => {RebindBinding(GameInput.Bindings.CameraRight); });
       
        lightAttackButton.AddListener(() => {RebindBinding(GameInput.Bindings.LightAttack); });   
        strongAttackButton.AddListener(() => { RebindBinding(GameInput.Bindings.StrongAttack); });
        dashButton.AddListener(() => { RebindBinding(GameInput.Bindings.Dash); });
        interactButton.AddListener(() => { RebindBinding(GameInput.Bindings.Interact); });
       
        ability1Button.AddListener(() => {RebindBinding(GameInput.Bindings.Ability1); });
        ability2Button.AddListener(() => {RebindBinding(GameInput.Bindings.Ability2); });
        ability3Button.AddListener(() => {RebindBinding(GameInput.Bindings.Ability3); });

        UpdateVisual();
        HidePressToRebindKey();
        
        backButton.AddListener(OnBackButtonClicked);
        
        container.gameObject.SetActive(false);
        gameObject.SetActive(false);
        
        GameInput.Instance.OnPauseToggle += OnPause;
    }

    private void ToggleButtons(bool enable)
    {
        if (enable)
        {
            moveUpButton.Enable();
            moveDownButton.Enable();
            moveLeftButton.Enable();
            moveRightButton.Enable();
            cameraLeftButton.Enable();
            cameraRightButton.Enable();
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
            cameraLeftButton.Disable();
            cameraRightButton.Disable();
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

    private void OnDestroy()
    {
        GameInput.Instance.OnPauseToggle -= OnPause;
    }

    private void OnPause()
    {
        if (OptionsUI.Instance.gameObject.activeSelf || PauseMenu.Instance.gameObject.activeSelf)
            return;
        if(Hide())
            OptionsUI.Instance.Show();
    }
    
    private void OnBackButtonClicked()
    {
        if(Hide())
            OptionsUI.Instance.Show();
    }

    protected override void Activate()
    {
        ToggleButtons(true);
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
        
        cameraLeftButton.buttonText.text = GameInput.Instance.GetBindingText(GameInput.Bindings.CameraLeft);
        cameraRightButton.buttonText.text = GameInput.Instance.GetBindingText(GameInput.Bindings.CameraRight);
        
        lightAttackButton.buttonText.text = GameInput.Instance.GetBindingText(GameInput.Bindings.LightAttack);
        strongAttackButton.buttonText.text = GameInput.Instance.GetBindingText(GameInput.Bindings.StrongAttack);
        dashButton.buttonText.text = GameInput.Instance.GetBindingText(GameInput.Bindings.Dash);
        interactButton.buttonText.text = GameInput.Instance.GetBindingText(GameInput.Bindings.Interact);
        
        ability1Button.buttonText.text = GameInput.Instance.GetBindingText(GameInput.Bindings.Ability1);
        ability2Button.buttonText.text = GameInput.Instance.GetBindingText(GameInput.Bindings.Ability2);
        ability3Button.buttonText.text = GameInput.Instance.GetBindingText(GameInput.Bindings.Ability3);

        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }

    private void ShowPressToRebindKey()
    {
        pressToRebindKeyTransform.gameObject.SetActive(true);
    }
    private void HidePressToRebindKey()
    {
        pressToRebindKeyTransform.gameObject.SetActive(false);
    }

    private void RebindBinding(GameInput.Bindings binding)
    {
        ShowPressToRebindKey();
        GameInput.Instance.RebindBinding(binding, () => {
            HidePressToRebindKey();
            UpdateVisual();
        }); 
    }
}

