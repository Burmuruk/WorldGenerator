using System.Threading;
using System.Timers;
using Unity.VisualScripting;
using UnityEngine;
using WorldG.level;

namespace WorldG.Control
{
    public class PlayerController : MonoBehaviour
    {
        #region variables
        [SerializeField] private float _clickGap = .1f;
        [SerializeField] float zoomSpeed = 3;
        [SerializeField] float zoomSmooth = 1;

        LevelGenerator level;
        private float _lastClick = 0;
        private bool _canWork = true;
        private bool _isWorking = false;
        ISelectable selectable = null;
        MinionsManager _minionsManager;
        SynchronizationContext _syncContext;
        (int id, int clicks, IClickable clickable) clickedItem = default;
        (int id, ISelectable selectable) selected;
        Vector3 zoomCurSpeed = Vector3.zero;

        public float cameraSpeed = 3;

        PoolManager pool;
        #endregion

        public Topping TemporalPiece { get; set; }

        private void Awake()
        {
            level = FindObjectOfType<LevelGenerator>();
            _minionsManager = FindObjectOfType<MinionsManager>();
            _syncContext = SynchronizationContext.Current;
            pool = FindObjectOfType<PoolManager>();
        }

        private void Start()
        {
            Invoke("SetInitialPointCamera", .5f);
        }

        private void Update()
        {
            if (TemporalPiece.Prefab)
            {
                RaycastHit hit;

                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 200))
                {
                    var piece = level.GetPieceInfo(hit.collider.transform.position);
                    if (!piece.Topping.Prefab && piece.Type != SideType.Water)
                        TemporalPiece.Prefab.transform.position = hit.transform.position + Vector3.up * 1;

                    Debug.DrawRay(TemporalPiece.Prefab.transform.position, Vector3.up * 10);
                }

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    pool.DisableUnamanagedTopping(TemporalPiece);
                    TemporalPiece = default;
                }
            }

            if (Input.mousePosition.x >= Screen.width - 20)
                Camera.main.transform.Translate(Vector3.right * Time.deltaTime * cameraSpeed);
            if (Input.mousePosition.x <= 20)
                Camera.main.transform.Translate(Vector3.left * Time.deltaTime * cameraSpeed);
            if (Input.mousePosition.y >= Screen.height - 20)
                Camera.main.transform.Translate(Vector3.up * Time.deltaTime * cameraSpeed);
            if (Input.mousePosition.y <= 20)
                Camera.main.transform.Translate(Vector3.down * Time.deltaTime * cameraSpeed);

            if (Input.GetMouseButtonUp(0))
            {
                RaycastHit hit;

                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 200))
                {
                    if (hit.collider.gameObject.GetComponent<Citizen>() is var m && m != null)
                    {
                        _minionsManager.SelectMinion(m);
                        m.Click();
                    }
                    else if (hit.collider.gameObject.GetComponent<IClickable>() is var c && c != null)
                    {
                        if (c.IsWorking) return;

                        InteractWithStructure(c, hit.colliderInstanceID);
                    }
                    else
                    {
                        
                        _minionsManager.DeSelect();
                        var cord = level.RemoveOffset(hit.collider.transform.position);
                        //PieceData.ChangeColor(pieces[cord.x, cord.y], SideType.Mudd);
                        level.SetRoad(cord);
                    }
                }
                else
                    _minionsManager.DeSelect();
            }

            if (Input.GetMouseButtonDown(1))
            {
                RaycastHit hit;

                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 200))
                {
                    if (hit.collider.gameObject.GetComponent<ISelectable>() is var s && s != null)
                    {
                        if (s.IsSelected)
                        {
                            s.Deselect();
                            selected = (-1, null);
                        }
                        else 
                        {
                            selected.selectable?.Deselect();
                            selected = (hit.colliderInstanceID, s);
                            s.Select();

                            _minionsManager.SetTarget(s, hit.collider.transform.position);
                        
                            //selectable = s;
                            //selectable.OnDeselection += () => selectable = null;
                        }
                    }
                    else
                    {
                        _minionsManager.SetTarget(null, hit.collider.transform.position);
                        selected = (-1, null);
                    }
                }
            }

            var wheel = Input.mouseScrollDelta.y;
            if (wheel != 0)
            {
                Camera.main.transform.Translate(new Vector3(0, wheel * zoomSpeed * Time.deltaTime * -1, 0), Space.World);
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