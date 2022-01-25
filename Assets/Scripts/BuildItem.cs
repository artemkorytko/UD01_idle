using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildItem : MonoBehaviour
{
    [SerializeField] private BuildingItemContainer _itemsContainer;
    [SerializeField] private Transform _modelPoint;
    private BuildingButtonController _buttonController;
    private GameObject _currentModel;
    private Coroutine _timerCoroutine;


    public bool IsUnlock { get; private set; }
    public int Level { get; private set; }

    public event Action<int> OnProcess;
    public event Action<float> OnBuildUpgrade;

    private void Awake()
    {
        _buttonController = GetComponentInChildren<BuildingButtonController>(true);
    }
    public void Initialize(bool isUnlock, int level)
    {
        IsUnlock = isUnlock;
        Level = level;
        GetComponentInChildren<Canvas>().worldCamera = Camera.main;
        if (isUnlock && Level >= 0) SetModel(level);
        UpdateButtonState();
        GameManager.Instance.OnMoneyValueChange += _buttonController.OnMoneyValueChange;
        _buttonController.OnButtonClick += Upgrade;
    }

    private void Upgrade()
    {
        if(!IsUnlock)
        {
            IsUnlock = true;
            UpdateButtonState();
            SetModel(Level);
            OnBuildUpgrade?.Invoke(_itemsContainer.UnlockPrice);
        }
        else if(_itemsContainer.IsUpgradeExist(Level))
        {
            Level++;
            UpdateButtonState();
            SetModel(Level);
            OnBuildUpgrade?.Invoke(GetPrice(Level));
        }
    }

    private void UpdateButtonState()
    {
        if(!IsUnlock)
        {
            _buttonController.UpdateButton("BUY", _itemsContainer.UnlockPrice);
        }
        else if(_itemsContainer.IsUpgradeExist(Level))
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
        return (float) Math.Round(_itemsContainer.UpgradePrice * Mathf.Pow(_itemsContainer.PriceMultiplier, level),2); 
    }

    private void SetModel(int level)
    {
        var buildItemConfig = _itemsContainer.GetUpgrade(level);
        if(_currentModel != null)
        {
            Destroy(_currentModel);
        }
       _currentModel = Instantiate(buildItemConfig.Model);
        _currentModel.transform.parent = _modelPoint;
        _currentModel.transform.localPosition = Vector3.zero;

        if (_timerCoroutine != null)
        {
            StopCoroutine(_timerCoroutine);
        }
        _timerCoroutine = StartCoroutine(Timer());
        OnProcess?.Invoke(_itemsContainer.GetUpgrade(Level).ProcessResult);
    }

    private IEnumerator Timer()
    {
        while(true)
        {
            yield return new WaitForSeconds(1);
        }
    }
    private void OnDestroy()
    {
        GameManager.Instance.OnMoneyValueChange -= _buttonController.OnMoneyValueChange;
        _buttonController.OnButtonClick -= Upgrade;
    }
}
