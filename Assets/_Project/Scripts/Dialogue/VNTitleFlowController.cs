using ProjectAllTime.VN.Dialogue;
using ProjectAllTime.VN.MetaProgress;
using ProjectAllTime.VN.Records;
using ProjectAllTime.VN.Records.UI;
using ProjectAllTime.VN.SaveLoad;
using ProjectAllTime.VN.Settings;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Yarn.Unity;

namespace ProjectAllTime.VN.Title
{
    /// <summary>Coordinates the title with the existing scene services; no persistent data is owned here.</summary>
    [DefaultExecutionOrder(10)]
    public sealed class VNTitleFlowController : MonoBehaviour
    {
        [SerializeField] private DialogueRunner runner;
        [SerializeField] private VNInteractionGate gate;
        [SerializeField] private VNConvenienceController convenience;
        [SerializeField] private VNConvenienceInputRouter router;
        [SerializeField] private VNConvenienceModalController modals;
        [SerializeField] private VNSaveLoadController saves;
        [SerializeField] private VNSettingsRuntimeBootstrap settings;
        [SerializeField] private VNMetaProgressRuntimeBootstrap meta;
        [SerializeField] private VNRecordsRuntimeBootstrap records;
        [SerializeField] private GameObject titleLayer;
        [SerializeField] private GameObject gameMenu;
        [SerializeField] private GameObject confirmation;
        [SerializeField] private TMP_Text confirmationText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private Button newGame, continueGame, loadGame, recordsButton, settingsButton, quitButton;
        [SerializeField] private Button resumeButton, titleButton, gameQuitButton, confirmButton, cancelButton;
        [SerializeField] private CanvasGroup dialogueLayer, quickControls;

        private enum State { Title, Playing, Menu, ConfirmTitle, ConfirmQuit, Loading, Transition }
        private State state;
        private VNSaveSlotKey latest;
        private bool loadingFromTitle;
        private GameObject lastTitleSelection;
        private static string recoveryMessage;
        private CanvasGroup titleGroup;

        private void Awake()
        {
            state = State.Title;
            titleGroup = titleLayer != null ? titleLayer.GetComponent<CanvasGroup>() : null;
            gate?.SetTitleFlowBlocksGameplay(true);
            // Yarn normally dismisses this presenter after automatic startup.
            // With title-first startup there is no old line to dismiss, so put
            // its visual state at the load barrier's initial quiescent state.
            if (runner != null)
                foreach (var presenter in runner.DialoguePresenters)
                    if (presenter is LinePresenter line)
                    {
                        if (line.canvasGroup != null) line.canvasGroup.alpha = 0;
                        if (line.lineText != null) line.lineText.maxVisibleCharacters = 0;
                    }
            SetGameplayVisible(false);
            if (titleLayer != null) titleLayer.SetActive(true);
            if (gameMenu != null) gameMenu.SetActive(false);
            SetGroupInteraction(gameMenu, false);
            SetGroupInteraction(confirmation, false);
            if (confirmation != null) confirmation.SetActive(false);
        }
        private void OnEnable()
        {
            if (router != null) router.UnhandledCancel += HandleCancel;
            if (saves != null) { saves.LoadStateChanged += OnLoadState; saves.LoadExecutionCompleted += OnLoadCompleted; }
        }
        private void OnDisable()
        {
            if (router != null) router.UnhandledCancel -= HandleCancel;
            if (saves != null) { saves.LoadStateChanged -= OnLoadState; saves.LoadExecutionCompleted -= OnLoadCompleted; }
        }
        private void Start()
        {
            newGame.onClick.AddListener(NewGame); continueGame.onClick.AddListener(Continue);
            loadGame.onClick.AddListener(OpenLoad); recordsButton.onClick.AddListener(OpenRecords);
            settingsButton.onClick.AddListener(OpenSettings); quitButton.onClick.AddListener(AskQuit);
            resumeButton.onClick.AddListener(Resume); titleButton.onClick.AddListener(AskTitle);
            gameQuitButton.onClick.AddListener(AskQuit); confirmButton.onClick.AddListener(Confirm);
            cancelButton.onClick.AddListener(CancelConfirmation);
            RefreshTitle(); Select(newGame);
            if (!string.IsNullOrEmpty(recoveryMessage)) { statusText.text = recoveryMessage; recoveryMessage = null; }
        }
        private void Update()
        {
            if (state != State.Title) return;
            bool modalOpen = (saves != null && saves.IsModalOpen) || (modals != null && modals.IsConvenienceModalOpen);
            if (!modalOpen && lastTitleSelection != null)
            {
                SetTitleModalObscured(false);
                EventSystem.current?.SetSelectedGameObject(lastTitleSelection);
                lastTitleSelection = null;
                RefreshTitle();
            }
        }
        private void RefreshTitle()
        {
            bool core = runner != null && runner.YarnProject != null && gate != null && saves != null && saves.Repository != null && meta != null && meta.IsInitialized;
            newGame.interactable = core;
            loadGame.interactable = core;
            continueGame.interactable = core && saves.TryFindLatestCompatibleSlot(out latest);
            settingsButton.interactable = settings != null && settings.IsInitialized && modals != null;
            recordsButton.interactable = records != null && records.IsInitialized && modals != null;
            if (!core) statusText.text = "게임 서비스를 준비하지 못했습니다.";
            else statusText.text = "";
        }
        private async void NewGame()
        {
            if (!CanUseTitle() || !newGame.interactable) return;
            state = State.Transition;
            SetTitleInteraction(false);
            convenience.SetAutoEnabled(false); convenience.SetSkipEnabled(false);
            saves.PlayTimeTracker.TrySetPlayedSeconds(0);
            SetGameplayVisible(true);
            try
            {
                await runner.StartDialogue("PROLOGUE_S001_START");
                titleLayer.SetActive(false); gate.SetTitleFlowBlocksGameplay(false);
                state = State.Playing;
            }
            catch (System.Exception exception)
            {
                Debug.LogError("New Game could not start: " + exception.Message, this);
                recoveryMessage = "새 게임을 시작하지 못했습니다.";
                SceneManager.LoadScene(gameObject.scene.buildIndex);
            }
        }
        private void Continue()
        {
            if (!CanUseTitle() || !continueGame.interactable) return;
            var result = saves.Load(latest);
            if (!result.Succeeded) { RefreshTitle(); statusText.text = "저장 데이터를 불러올 수 없습니다."; }
        }
        private void OpenLoad()
        {
            if (!CanUseTitle() || !loadGame.interactable) return;
            lastTitleSelection = loadGame.gameObject;
            saves.BeginModalInputSuppression(); saves.OpenLoad();
            if (saves.IsModalOpen) SetTitleModalObscured(true);
            SelectFirst(Object.FindFirstObjectByType<VNSaveLoadModal>());
        }
        private void OpenSettings()
        {
            if (!CanUseTitle() || !settingsButton.interactable) return;
            lastTitleSelection = settingsButton.gameObject;
            if (!modals.TryOpenSettings()) { lastTitleSelection = null; statusText.text = "설정을 열 수 없습니다."; }
            else { SetTitleModalObscured(true); SelectFirst(Object.FindFirstObjectByType<VNSettingsModal>()); }
        }
        private void OpenRecords()
        {
            if (!CanUseTitle() || !recordsButton.interactable) return;
            lastTitleSelection = recordsButton.gameObject;
            if (!modals.TryOpenRecords()) { lastTitleSelection = null; statusText.text = "기록을 열 수 없습니다."; }
            else { SetTitleModalObscured(true); SelectFirst(Object.FindFirstObjectByType<VNRecordsModal>()); }
        }
        private void OnLoadState(bool loading)
        {
            if (!loading || state != State.Title) return;
            loadingFromTitle = true; state = State.Loading;
            SetTitleInteraction(false);
            statusText.text = "불러오는 중...";
        }
        private void OnLoadCompleted(VNSaveLoadOperationResult result)
        {
            if (!loadingFromTitle) return;
            loadingFromTitle = false;
            if (result != null && result.Succeeded)
            {
                titleLayer.SetActive(false); SetGameplayVisible(true); gate.SetTitleFlowBlocksGameplay(false);
                state = State.Playing;
            }
            else
            {
                state = State.Transition;
                recoveryMessage = "불러오기에 실패했습니다. 타이틀로 돌아왔습니다.";
                SceneManager.LoadScene(gameObject.scene.buildIndex);
            }
        }
        private bool HandleCancel()
        {
            if (state == State.Playing)
            {
                if (saves.IsLoadInProgress) return true;
                state = State.Menu; convenience.SetAutoEnabled(false); convenience.SetSkipEnabled(false);
                SetGameplayInputEnabled(false);
                gate.SetTitleFlowBlocksGameplay(true);
                SetGroupInteraction(gameMenu, true);
                gameMenu.SetActive(true);
                Select(resumeButton);
                return true;
            }
            if (state == State.Menu) { Resume(); return true; }
            if (state == State.ConfirmTitle || state == State.ConfirmQuit) { CancelConfirmation(); return true; }
            return state != State.Playing;
        }
        private void Resume()
        {
            if (state != State.Menu) return;
            SetGroupInteraction(gameMenu, false);
            gameMenu.SetActive(false);
            SetGameplayInputEnabled(true);
            gate.SetTitleFlowBlocksGameplay(false);
            state = State.Playing;
        }
        private void AskTitle() { if (state != State.Menu || saves.IsLoadInProgress) return; state = State.ConfirmTitle; ShowConfirmation("타이틀로 돌아가시겠습니까? 저장하지 않은 진행은 사라집니다."); }
        private void AskQuit() { if (state != State.Menu && !CanUseTitle()) return; state = State.ConfirmQuit; ShowConfirmation("게임을 종료하시겠습니까?"); }
        private void ShowConfirmation(string message)
        {
            if (gameMenu != null && gameMenu.activeSelf) SetGroupInteraction(gameMenu, false);
            SetTitleInteraction(false);
            confirmationText.text = message;
            SetGroupInteraction(confirmation, true);
            confirmation.SetActive(true);
            Select(cancelButton);
        }
        private void CancelConfirmation()
        {
            if (state != State.ConfirmTitle && state != State.ConfirmQuit) return;
            SetGroupInteraction(confirmation, false);
            confirmation.SetActive(false);
            state = gameMenu.activeSelf ? State.Menu : State.Title;
            if (state == State.Menu)
            {
                SetGroupInteraction(gameMenu, true);
                Select(resumeButton);
            }
            else
            {
                SetTitleInteraction(true);
                Select(quitButton);
            }
        }
        private void Confirm()
        {
            if (state == State.ConfirmTitle)
            {
                state = State.Transition;
                SetGroupInteraction(confirmation, false);
                SceneManager.LoadScene(gameObject.scene.buildIndex);
            }
            else if (state == State.ConfirmQuit)
            {
                state = State.Transition;
                SetGroupInteraction(confirmation, false);
                Application.Quit();
            }
        }
        private void SetGameplayVisible(bool visible)
        {
            foreach (var group in new[] { dialogueLayer, quickControls }) if (group != null)
                group.alpha = visible ? 1 : 0;
            SetGameplayInputEnabled(visible);
        }
        private void SetGameplayInputEnabled(bool enabled)
        {
            foreach (var group in new[] { dialogueLayer, quickControls }) if (group != null)
            {
                bool canInteract = enabled && group.alpha > 0.001f;
                group.interactable = canInteract;
                group.blocksRaycasts = canInteract;
            }
        }
        private void SetTitleInteraction(bool enabled)
        {
            if (titleGroup == null) return;
            bool canInteract = enabled && titleGroup.alpha > 0.001f;
            titleGroup.interactable = canInteract;
            titleGroup.blocksRaycasts = canInteract;
        }
        private static void SetGroupInteraction(GameObject root, bool enabled)
        {
            if (root == null) return;
            var group = root.GetComponent<CanvasGroup>();
            if (group == null) return;
            bool canInteract = enabled && group.alpha > 0.001f;
            group.interactable = canInteract;
            group.blocksRaycasts = canInteract;
        }
        private static void Select(Button button) { if (EventSystem.current != null && button != null) EventSystem.current.SetSelectedGameObject(button.gameObject); }
        private void SetTitleModalObscured(bool obscured)
        {
            if (titleGroup == null) return;
            titleGroup.alpha = obscured ? 0 : 1;
            titleGroup.interactable = !obscured;
            titleGroup.blocksRaycasts = !obscured;
        }
        private bool CanUseTitle() => state == State.Title && (saves == null || !saves.IsModalOpen && !saves.IsLoadInProgress) && (modals == null || !modals.IsConvenienceModalOpen);
        private static void SelectFirst(Component modal)
        {
            if (modal == null || EventSystem.current == null) return;
            foreach (var selectable in modal.GetComponentsInChildren<Selectable>(true))
                if (selectable.isActiveAndEnabled && selectable.IsInteractable())
                { EventSystem.current.SetSelectedGameObject(selectable.gameObject); return; }
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
