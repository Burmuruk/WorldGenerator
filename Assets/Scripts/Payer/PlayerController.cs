using System.Threading;
using System.Timers;
using UnityEngine;
using WorldG.level;

namespace WorldG.Control
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float _clickGap = .1f;

        LevelGenerator level;
        private float _lastClick = 0;
        private bool _canWork = true;
        private bool _isWorking = false;
        ISelectable selectable = null;
        MinionsManager _minionsManager;
        SynchronizationContext _syncContext;
        (int id, int clicks, IClickable clickable) clickedItem = default;
        (int id, ISelectable) selected;

        private void Awake()
        {
            level = FindObjectOfType<LevelGenerator>();
            _minionsManager = FindObjectOfType<MinionsManager>();
            _syncContext = SynchronizationContext.Current;
        }

        private void Start()
        {
            Invoke("SetInitialPointCamera", .5f);
        }

        private void Update()
        {
            if (Input.GetMouseButtonUp(0))
            {
                RaycastHit hit;

                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 200))
                {
                    if (hit.collider.gameObject.GetComponent<IClickable>() is var c && c != null)
                    {
                        if (c.IsWorking) return;

                        InteractWithStructure(c, hit.colliderInstanceID);
                    }
                    else
                    {
                        var cord = level.RemoveOffset(hit.collider.transform.position);
                        //PieceData.ChangeColor(pieces[cord.x, cord.y], SideType.Mudd);
                        level.SetRoad(cord);
                    }
                }
            }

            if (Input.GetMouseButtonDown(1))
            {
                RaycastHit hit;

                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 200))
                {
                    if (hit.collider.gameObject.GetComponent<ISelectable>() is var s && s != null)
                    {
                        if (s.IsSelected) return;

                        s.Select();
                        _minionsManager.SetTarget(s, hit.collider.transform.position);
                        
                        //selectable = s;
                        //selectable.OnDeselection += () => selectable = null;
                    }

                    if (selected.id != hit.colliderInstanceID)
                    {
                        selected.Item2?.Deselect();
                        selected = (hit.colliderInstanceID, s);
                    }
                    else
                    {
                        _minionsManager.SetTarget(null, hit.collider.transform.position);
                        selected = (hit.colliderInstanceID, s);
                    }
                }
            }
        }

        private void InteractWithStructure(IClickable clickable, int obj)
        {
            if (clickedItem.id == obj)
                clickedItem.clicks++;
            else
                clickedItem = (obj, 1, clickable);

            _lastClick = Time.time;
            var timer = new System.Timers.Timer(_clickGap);
            timer.AutoReset = false;
            timer.Elapsed += (object source, ElapsedEventArgs args) =>
                {
                    if (clickedItem.clicks == 1)
                        _syncContext.Post((d) => clickable.Click(), null);
                    else
                        _syncContext.Post((d) => clickable.DoubleClick(), null);

                    clickedItem = default;
                    timer.Dispose();
                };
            timer.Start();

        }

        private void SetInitialPointCamera()
        {
            Camera.main.transform.position = level.StartPoint + Vector3.up * 10;
        }

        private void Deselect()
        {
            //if (clickedItem.id != 0)
            //{
            //    clickedItem.clickable.d
            //}
        }
    } 
}