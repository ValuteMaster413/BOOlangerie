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
        private Order wantedOrder;

        public GameState State { get; private set; }
        public CustomerData CurrentCustomer => customers[customerIndex];
        public string OrderKnot => $"{CurrentCustomer.id}_r{roundIndex + 1}";
        public string ResultKnot => $"{OrderKnot}_result";

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

        // TODO: remove the space debug key
        private void Update()
        {
            if (Keyboard.current == null || !Keyboard.current.spaceKey.wasPressedThisFrame) return;

            if (State == GameState.Prep) Serve(null);
            else DialogueFinished();
        }

        public void SetWantedOrder(Order wanted)
        {
            wantedOrder = wanted;
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

            OrderResult result = OrderEvaluator.Evaluate(wantedOrder, made);
            Debug.Log($"Served {CurrentCustomer.id}: {result}");
            OrderServed?.Invoke(result);
            SetState(GameState.Reaction);
        }

        private void NextRound()
        {
            roundIndex++;
            if (roundIndex >= CurrentCustomer.rounds)
            {
                roundIndex = 0;
                customerIndex++;
            }

            if (customerIndex >= customers.Length)
            {
                SceneManager.LoadScene(endingScene);
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