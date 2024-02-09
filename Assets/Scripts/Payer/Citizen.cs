using System.Collections;
using UnityEngine;
using WorldG.Architecture;
using WorldG.Stats;
using WorldG.UI;

namespace WorldG.Control
{
    public class Citizen : Minion, IClickable
    {
        private Resource target;
        [SerializeField] private int damage = 0;
        [SerializeField] private int farmAmount = 30;
        [SerializeField] private float farmingRate = 2;
        InventoryManager inventory;
        MinionMenuController menu;

        private bool isFarming = false;

        protected override void Awake()
        {
            base.Awake();

            inventory = FindObjectOfType<InventoryManager>();
        }

        public override void Deselect()
        {
            base.Deselect();

            if (menu)
            {
                menu.HideMenu();
                menu = null;
            }
        }

        public override void SetWork(object args)
        {
            if (IsWorking) return;
            _isWorking = true;
            target = (Resource)args;

            //StopActions();

            _patrolController.OnPatrolFinished += MoveToTarget;
            MoveTo(target.transform.position);
        }

        protected override void MoveToTarget()
        {
            _patrolController.OnPatrolFinished -= MoveToTarget;
            if (!target) 
                { _isWorking = false; return; }
            print("SecondRound");
            var piece = level.GetPieceInfo(target.transform.position);

            if (!piece.Topping.Patrol) 
                { _isWorking = false; return; }

            Debug.DrawRay(target.transform.position, Vector3.up * 10, Color.white, 10);
            _patrolController.Mover.OnFinished -= _patrolController.Execute_Tasks;
            _patrolController.Mover.OnFinished += piece.Topping.Patrol.Execute_Tasks;

            piece.Topping.Patrol.OnPatrolFinished += () =>
            {
                _patrolController.Mover.OnFinished -= piece.Topping.Patrol.Execute_Tasks;
                _patrolController.Mover.OnFinished += _patrolController.Execute_Tasks;
                print("Farming");
                if (!isFarming)
                    StartCoroutine(Farm(piece));
            };

            piece.Topping.Patrol.Mover = GetComponent<Movement>();
            piece.Topping.Patrol.Initialize();
            piece.Topping.Patrol.Execute_Tasks();
            //_patrolController.fin
        }

        IEnumerator Farm(Piece piece)
        {
            isFarming = true;
            ResourceWorker worker = null;

            foreach (var component in piece.Components)
            {
                if (component is ResourceWorker w)
                {
                    worker = w;
                    break;
                }
            }

            if (worker == null) {
                _isWorking = false; 
                isFarming = false;
                yield break; 
            }

            ResourceType type = piece.ToppingType switch
            {
                ToppingType.Tree => ResourceType.Wood,
                ToppingType.Rock => ResourceType.Stone,
                ToppingType.Mill => ResourceType.Food,
                _ => ResourceType.None

            };

            try
            {
                while (IsWorking)
                {
                    yield return new WaitForSeconds(farmingRate);

                    var amount = worker.TakeResource(farmAmount);

                    inventory.AddResource(type, amount);

                    if (amount != farmAmount)
                        _isWorking = false;
                }
            }
            finally {  _isWorking = false; isFarming = false; }

            yield break;
        }

        public void Click()
        {
            menu = FindObjectOfType<MinionMenuController>();
            menu.ShowMenu();
        }

        public void DoubleClick() { }
    }
}
