using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

public class BuildItem : MonoBehaviour
{
    public event Action<int> OnProcess;
    public event Action<float> OnBuildUpgrade; 
    
    [SerializeField] private BuildingItemsContainer _itemsContainer;
    [SerializeField] private Transform _modelPoint;
    [SerializeField] private BuildingButtonController _buttonController;
    private GameObject _currentModel;
    private Coroutine _timerCoroutine;
    
    public bool IsUnlocked { get; private set; }
    public int Level { get; private set; }

    private void Awake()
    {
        _buttonController = GetComponentInChildren<BuildingButtonController>(true);
    }

    public void Initialize(bool unlockState, int level)
    {
        IsUnlocked = unlockState;
        Level = level;
        GetComponentInChildren<Canvas>().worldCamera = Camera.main;
        
        _buttonController.Initialize();
        
        if (unlockState && level >= 0)
            SetModel(level);
        UpdateButtonState();
        GameManager.Inst.OnMoneyValueChange += _buttonController.OnMoneyChanged;
        _buttonController.OnButtonClick += DoUpgrade;
    }

    private void DoUpgrade()
    {
        if (!IsUnlocked)
        {
            IsUnlocked = true;
            //OnBuildUpgrade?.Invoke(_itemsContainer.UnlockPrice);
        }
        else if (_itemsContainer.IsUpgradeExist(Level + 1))
        {
            Level++;
            //OnBuildUpgrade?.Invoke(GetPrice(Level));
        }
        OnBuildUpgrade?.Invoke(GetPrice(Level));
        UpdateButtonState();
        SetModel(Level);
    }

    private void UpdateButtonState()
    {
        if (!IsUnlocked)
        {
            _buttonController.UpdateButton("BUY", _itemsContainer.UnlockPrice);
        }
        else if (_itemsContainer.IsUpgradeExist(Level))
        {
            _buttonController.UpdateButton("UPGRADE", GetPrice(Level));
        }
        else
        {
            _buttonController.gameObject.SetActive(false);
        }
    }

    private float GetPrice(int level)
    {
        return _itemsContainer.StartUpgradePrice * Mathf.Pow(_itemsContainer.PriceMultiplier, level);
    }

    private void SetModel(int level)
    {
        var buildItemConfig = _itemsContainer.GetUpgrade(level);
        if (_currentModel != null)
        {
            Destroy(_currentModel);
        }
        _currentModel = Instantiate(buildItemConfig.Model);
        _currentModel.transform.parent = _modelPoint;
        _currentModel.transform.localPosition = Vector3.zero;
        _currentModel.transform.rotation = _modelPoint.transform.rotation;
        
        if(_timerCoroutine != null) StopCoroutine(_timerCoroutine);
        _timerCoroutine = StartCoroutine(Timer());
    }

    private IEnumerator Timer()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            OnProcess?.Invoke(_itemsContainer.GetUpgrade(Level).ProcessCof);
        }
    }

    private void OnDestroy()
    {
        GameManager.Inst.OnMoneyValueChange -= _buttonController.OnMoneyChanged;
        _buttonController.OnButtonClick -= DoUpgrade;
    }
}
