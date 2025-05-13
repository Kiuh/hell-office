using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AnimatorsSwitcher;
using Assets.Scripts.Dialogue;
using Assets.Scripts.Level.Aims;
using Common;
using Employee.Needs;
using Level.Config;
using Level.GlobalTime;
using Location;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Level
{
    public struct GameLoseCause
    {
        public LoseGame.Cause Cause;
    }

    public struct AllEmployeesAtBadroom
    {
        public bool Value;
    }

    public struct AllEmployeesAtMeeting
    {
        public bool Value;
    }

    [AddComponentMenu("Scripts/Level/Level.Executor")]
    public class Executor : MonoBehaviour
    {
        [SerializeField]
        private TileBuilder.Controller tileBuilderController;

        [SerializeField]
        private Finances.Model financesModel;

        [SerializeField]
        private Shop.Controller shopController;

        [SerializeField]
        private AnimatorsSwitcherImpl animatorSwitcher;

        [SerializeField]
        private TransitionPanel.Model transitionPanel;

        [SerializeField]
        private NeedProviderManager needProviderManager;

        [SerializeField]
        private Location.EmployeeManager.Model employeeManager;

        [SerializeField]
        private GlobalTime.Model globalTime;

        [RequiredIn(PrefabKind.PrefabInstanceAndNonPrefabInstance)]
        [SerializeField]
        private LoseGamePanel.Model loseGamePanelModel;

        [SerializeField]
        private NavMeshSurfaceUpdater navMeshUpdater;

        [SerializeField]
        private Aims aims;

        [SerializeField]
        private DialogueExecutor dialogueExecutor;

        [SerializeField]
        private AllChildrenNeedModifiersApplier meetingStartNeedOverride;

        [SerializeField]
        private AllChildrenNeedModifiersApplier meetingEndNeedOverride;

        [SerializeField]
        private AllChildrenNeedModifiersApplier leaveNeedOverride;

        [SerializeField]
        private AllChildrenNeedModifiersApplier goToWorkNeedOverride;

        [SerializeField]
        private ConfigHandler configHandler;

        public event Action ActionEndNotify;

        private bool transitionPanelShown = false;
        private bool cutsceneMinTimeEnded = false;
        private bool isGameFinished = false;
        private bool isPreMeetingEnd = false;

        [SerializeField]
        private UnityEvent dayEnded;

        private void Awake()
        {
            tileBuilderController.BuiltValidatedOffice += CompleteMeeting;
        }

        private void Update()
        {
            IEnumerable<GameLoseCause> causes =
                DataProviderServiceLocator.FetchDataFromMultipleSources<GameLoseCause>();
            if (causes.Count() > 0 && !isGameFinished)
            {
                Execute(new LoseGame(causes.First().Cause));
            }
        }

        public void Execute(DayStart dayStart)
        {
            Result timeLockResult = globalTime.SetTimeScaleLock(this, 1f);
            timeLockResult.LogErrorIfFailure(
                "Cannot set time scale lock in DayStart."
            );
            navMeshUpdater.UpdateNavMesh();
            needProviderManager.InitGameMode();

            // NOTE: It's a temporary solution while we don't have proper save/load system.
            leaveNeedOverride.Unregister();
            meetingEndNeedOverride.Unregister();
            goToWorkNeedOverride.Register();

            _ = StartCoroutine(employeeManager.TurnOnAllEmployees(dayStart.EmployeeEnableDelay));

            financesModel.AddMoney(dayStart.MorningMoney);
            animatorSwitcher.SetAnimatorStates(typeof(DayStart));
            _ = StartCoroutine(DayStartRoutine(dayStart.Duration));
        }

        private IEnumerator DayStartRoutine(RealTimeSeconds time)
        {
            yield return new WaitForSecondsRealtime(time.Value);
            ActionEndNotify?.Invoke();
        }

        public void Execute(Dialogue dialogue)
        {
            animatorSwitcher.SetAnimatorStates(typeof(Dialogue));
            dialogueExecutor.ExecuteDialogue(dialogue.DialogueConfig);
        }

        public void CompleteDialogue()
        {
            ActionEndNotify?.Invoke();
        }

        public void Execute(PreMeeting preMeeting)
        {
            navMeshUpdater.UpdateNavMesh();
            needProviderManager.InitGameMode();

            meetingEndNeedOverride.Unregister();
            meetingStartNeedOverride.Register();

            isPreMeetingEnd = false;
            _ = StartCoroutine(PreMeetingDelay(preMeeting.MinWaitingTime));

            if (globalTime.IsLocked)
            {
                Result timeScaleLockResult = globalTime.RemoveTimeScaleLock(this);
                timeScaleLockResult.LogErrorIfFailure("Cannot remove timeScaleLock in Pre meeting");
            }

            this.CreateGate(
                new List<Func<bool>>()
                {
                    () =>
                        DataProviderServiceLocator
                            .FetchDataFromSingleton<AllEmployeesAtMeeting>()
                            .Value,
                    () => isPreMeetingEnd
                },
                new List<Action>()
                {
                    () =>
                    {
                        Result timeLockResult = globalTime.SetTimeScaleLock(this, 0f);
                        timeLockResult.LogErrorIfFailure(
                            "Cannot set time scale lock in preMeeting End."
                        );
                    },
                    ActionEndNotify.Invoke
                }
            );
        }

        private IEnumerator PreMeetingDelay(float time)
        {
            yield return new WaitForSecondsRealtime(time);
            isPreMeetingEnd = true;
        }

        public void Execute(Meeting meeting)
        {
            tileBuilderController.ChangeGameMode(TileBuilder.GameMode.Build);
            shopController.SetShopRooms(meeting.ShopRooms);
            shopController.SetShopEmployees(meeting.ShopEmployees);
            animatorSwitcher.SetAnimatorStates(typeof(Meeting));
        }

        public void CompleteMeeting()
        {
            navMeshUpdater.UpdateNavMesh();

            meetingStartNeedOverride.Unregister();
            meetingEndNeedOverride.Register();

            needProviderManager.InitGameMode();
            tileBuilderController.ChangeGameMode(TileBuilder.GameMode.Play);

            IEnumerable<NeedProvider> meeting_need_providers =
                needProviderManager.FindAllNeedProvidersOfType(NeedType.Meeting);
            foreach (NeedProvider need_provider in meeting_need_providers)
            {
                need_provider.ForceReleaseEmployeeIfAny();
            }

            ActionEndNotify?.Invoke();
        }

        public void Execute(Working working)
        {
            animatorSwitcher.SetAnimatorStates(typeof(Working));
            if (globalTime.IsLocked)
            {
                Result timeScaleLockResult = globalTime.RemoveTimeScaleLock(this);
                timeScaleLockResult.LogErrorIfFailure(
                    "Cannot remove timeScaleLock in Start Working"
                );
            }
            _ = StartCoroutine(WorkingTime(working.Duration));
        }

        private IEnumerator WorkingTime(InGameTime duration)
        {
            yield return new WaitForSeconds(duration.RealTimeSeconds.Value);
            Result timeLockResult = globalTime.SetTimeScaleLock(this, 0f);
            timeLockResult.LogErrorIfFailure("Cannot set time scale lock in End Working.");
            ActionEndNotify?.Invoke();
        }

        public void Execute(Cutscene cutscene)
        {
            transitionPanel.PanelText = cutscene.Text;
            animatorSwitcher.SetAnimatorStates(typeof(Cutscene));

            transitionPanelShown = false;

            this.CreateGate(
                new List<Func<bool>>() { () => transitionPanelShown, () => cutsceneMinTimeEnded },
                new List<Action>() { ActionEndNotify.Invoke, () => cutsceneMinTimeEnded = false }
            );
            _ = StartCoroutine(CutsceneRoutine(cutscene.Duration));
        }

        private IEnumerator CutsceneRoutine(RealTimeSeconds time)
        {
            yield return new WaitForSecondsRealtime(time.Value);
            cutsceneMinTimeEnded = true;
        }

        public void Execute(PreDayEnd preDayEnd)
        {
            goToWorkNeedOverride.Unregister();
            leaveNeedOverride.Register();

            if (globalTime.IsLocked)
            {
                Result timeScaleLockResult = globalTime.RemoveTimeScaleLock(this);
                timeScaleLockResult.LogErrorIfFailure(
                    "Cannot remove timeScaleLock in Start Working"
                );
            }

            this.CreateGate(
                new List<Func<bool>>()
                {
                    () =>
                        DataProviderServiceLocator
                            .FetchDataFromSingleton<AllEmployeesAtBadroom>()
                            .Value
                },
                new List<Action>()
                {
                    () =>
                    {
                        Result timeLockResult = globalTime.SetTimeScaleLock(this, 0f);
                        timeLockResult.LogErrorIfFailure(
                            "Cannot set time scale lock in End Working."
                        );
                    },
                    ActionEndNotify.Invoke
                }
            );
        }

        public void Execute(DayEnd dayEnd)
        {
            animatorSwitcher.SetAnimatorStates(typeof(DayEnd));
        }

        // Called by button continue on daily bill panel.
        public void CompleteDayEnd()
        {
            aims.DaysRemaining -= 1;
            if (financesModel.Money >= configHandler.Config.SoulsNeed)
            {
                Execute(new WinGame());
                return;
            }
            else if (aims.DaysRemaining <= 0)
            {
                Execute(new LoseGame(LoseGame.Cause.NoTimeLeft));
                return;
            }
            dayEnded.Invoke();
            ActionEndNotify?.Invoke();
        }

        // Called by TransitionPanel animator.
        public void TransitionPanelShown()
        {
            transitionPanelShown = true;
        }

        public void Execute(LoseGame loseGame)
        {
            isGameFinished = true;
            loseGamePanelModel.SetCause(loseGame.LoseCause);
            animatorSwitcher.SetAnimatorStates(typeof(LoseGame));
        }

        public void Execute(WinGame winGame)
        {
            isGameFinished = true;
            animatorSwitcher.SetAnimatorStates(typeof(WinGame));
        }

        public void Execute(LoadLevel loadLevel)
        {
            isGameFinished = false;
            tileBuilderController.LoadBuildingFromConfig(loadLevel.BuildingConfig);
            animatorSwitcher.SetAnimatorStates(typeof(LoadLevel));
            Result timeLockResult = globalTime.SetTimeScaleLock(this, 0f);
            timeLockResult.LogErrorIfFailure("Cannot set time scale lock in Load level.");
            ActionEndNotify?.Invoke();
        }
    }
}
