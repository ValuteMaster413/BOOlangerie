using System;
using BOO.Data;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace BOO.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private CustomerData[] customers;
        [SerializeField] private string endingScene = "Ending";

        private int customerIndex;
        private int roundIndex;

        public GameState State { get; private set; }
        public CustomerData CurrentCustomer => customers[customerIndex];
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

        // TODO: remove once the prep station calls Serve
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
            }
        }

        public void Serve(Order made)
        {
            if (State != GameState.Prep) return;

            OrderResult result = OrderEvaluator.Evaluate(CurrentRound, made);
            Debug.Log($"Served {CurrentCustomer.id}: {result}");
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
                if (SceneTransitionManager.Instance != null) SceneTransitionManager.Instance.LoadScene(endingScene);
                else SceneManager.LoadScene(endingScene);
                return;
            }

            SetState(GameState.Ordering);
        }

        private void SetState(GameState next)
        {
            State = next;
            Debug.Log($"State: {next} (customer {customerIndex}, round {roundIndex + 1})");
            StateChanged?.Invoke(next);
        }
    }
}