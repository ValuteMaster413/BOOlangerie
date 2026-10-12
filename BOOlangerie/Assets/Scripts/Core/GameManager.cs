using System;
using BOO.Data;
using BOO.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace BOO.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private CustomerData[] customers;
        [SerializeField] private string menuScene = "MainMenu";

        private int customerIndex;
        private int roundIndex;

        public GameState State { get; private set; }
        public CustomerData CurrentCustomer => customerIndex < customers.Length ? customers[customerIndex] : null;
        public string OrderKnot => $"{CurrentCustomer.id}_r{roundIndex + 1}";
        public string ResultKnot(OrderResult result) => $"{OrderKnot}_{result.ToString().ToLowerInvariant()}";

        public OrderRound CurrentRound
        {
            get
            {
                OrderRound[] rounds = CurrentCustomer.rounds;
                return roundIndex < rounds.Length ? rounds[roundIndex] : null;
            }
        }

        public Order LastServed { get; private set; }

        public event Action<GameState> StateChanged;
        public event Action<OrderResult> OrderServed;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            SetState(GameState.Intro);
        }

#if UNITY_EDITOR
        private void Update()
        {
            if (State != GameState.Prep || Keyboard.current == null) return;

            if (Keyboard.current.gKey.wasPressedThisFrame) Serve(FirstAcceptedOrder());   // a correct order
            else if (Keyboard.current.bKey.wasPressedThisFrame) Serve(null);              // a wrong order
        }

        private Order FirstAcceptedOrder()
        {
            OrderRound round = CurrentRound;
            return round != null && round.accepted.Length > 0 ? round.accepted[0] : null;
        }
#endif

        public void DialogueFinished()
        {
            switch (State)
            {
                case GameState.Intro:
                    SetState(GameState.Ordering);
                    break;
                case GameState.Ordering:
                    SetState(GameState.Prep);
                    break;
                case GameState.Reaction:
                    NextRound();
                    break;
                case GameState.Ending:
                    ShowThanks();
                    break;
            }
        }

        public void Serve(Order made)
        {
            if (State != GameState.Prep) return;

            OrderResult result = OrderEvaluator.Evaluate(CurrentRound, made);
            Debug.Log($"Served {CurrentCustomer.id}: {result}");
            LastServed = made;
            OrderServed?.Invoke(result);
            SetState(GameState.Reaction);
        }

        private void NextRound()
        {
            roundIndex++;
            if (roundIndex >= CurrentCustomer.rounds.Length)
            {
                roundIndex = 0;
                customerIndex++;
            }

            if (customerIndex >= customers.Length)
            {
                SetState(GameState.Ending);
                return;
            }

            SetState(GameState.Ordering);
        }

        private void ShowThanks()
        {
            ThanksUI thanks = FindAnyObjectByType<ThanksUI>(FindObjectsInactive.Include);

            if (thanks != null) thanks.Play(LoadMenu);
            else LoadMenu();
        }

        private void LoadMenu()
        {
            if (SceneTransitionManager.Instance != null) SceneTransitionManager.Instance.LoadScene(menuScene);
            else SceneManager.LoadScene(menuScene);
        }

        private void SetState(GameState next)
        {
            State = next;
            Debug.Log($"State: {next} (customer {customerIndex}, round {roundIndex + 1})");
            StateChanged?.Invoke(next);
        }
    }
}