using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using WorldG.level;

namespace WorldG.Control
{
    public class Building : MonoBehaviour, IClickable, IKillable, ISelectable
    {
        [SerializeField] CharacterType[] products;
        [SerializeField] GameObject buttons;
        private bool _isWorking = false;
        float health = 0;
        int id = 0;
        bool isSelected = false;
        Action onDeselection;
        [SerializeField] List<Minion> minions = new List<Minion>();

        LevelGenerator _levelGenerator;
        PoolManager _poolManager;
        MinionsManager _minionsManager;
        MinionPopUpMenu _minionPopUpMenu;
        Action onDie;

        public bool IsWorking { get => _isWorking; }

        public float Health => throw new NotImplementedException();
        public Action OnDie { get => onDie; set => onDie += value; }
        public int TotalMinions { get => minions.Count; }
        public int ID { get => id; }
        public List<Minion> Minions { get => minions; }

        public bool IsSelected => isSelected;

        public Action OnDeselection { get => onDeselection; set => onDeselection += value; }

        private void Awake()
        { 
            _levelGenerator = FindObjectOfType<LevelGenerator>();
            _minionsManager = FindObjectOfType<MinionsManager>();
            _poolManager = FindObjectOfType<PoolManager>();
            id = GetHashCode();
            _minionPopUpMenu = FindObjectOfType<MinionPopUpMenu>();
        }

        private void Start()
        {
            var button = buttons.transform.GetComponentsInChildren<MyButton>();

            foreach (var item in button)
            {
                item.OnClick += CreateMinion;
            }

            ConstraintSource source = new ConstraintSource();
            source.sourceTransform = Camera.main.transform;
            source.weight = 1.0f;
            var constrain = buttons.GetComponent<LookAtConstraint>();
            constrain.AddSource(source);
            constrain.rotationOffset = new Vector3(0, 180, 0);
        }

        private void Update()
        {
            if (isSelected)
                Debug.DrawRay(transform.position, Vector3.up * 8, Color.red);
        }

        public void Click()
        {
            if (IsWorking) return;

            try
            {
                _isWorking = true;
                ShowItems();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                _isWorking = false;
            }
        }

        private void SelectMinions()
        {
            _minionsManager.SelectGroup(this);
        }

        private void SelectMinion (Minion minion)
        {
            _minionsManager.SelectMinion(minion);
        }

        public void DoubleClick()
        {
            print("Double");
            if (IsWorking || TotalMinions <= 0) return;
            _isWorking = true;

            SelectMinions();

            _isWorking = false;
        }

        public void CreateMinion(CharacterType type)
        {
            var pos = _levelGenerator.RemoveOffset(transform.position);
            var nextPos = _levelGenerator.GetOffset(_levelGenerator.MovePosition(pos, 2));

            var character = _levelGenerator.SetCharacter(nextPos, type);

            minions.Add(character.Prefab.gameObject.GetComponent<Minion>());
        }

        private void ShowItems()
        {
            var pos = transform.position + new Vector3(2, 2 , 1);
            _minionPopUpMenu.SetPlace(pos, CreateMinion, id, products);
            //buttons.transform.gameObject.SetActive(!buttons.activeSelf);

        }

        private void Die()
        {
            buttons.SetActive(false);
            OnDie?.Invoke();
        }

        public void SetDamage(float damage)
        {
            health -= damage;

            if (health < 0)
            {
                OnDie?.Invoke();
                health = 0;
            }
        }

        public void Select()
        {
            
            isSelected = true;
        }

        public void Deselect()
        {
            isSelected = false;
        }
    }

    public interface IClickable
    {
        public bool IsWorking { get; }

        void Click();
        void DoubleClick();
    }
}

public interface IKillable
{
    public float Health { get; }
    public Action OnDie { get; set; }

    public void SetDamage(float damage);
}
