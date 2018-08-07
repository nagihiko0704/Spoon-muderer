﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    private Money _money;
    private float _m;     // 재화
    private Money _click;
    private float _c;     // 클릭 당 얻는 재화
    Text moneyText;           // 현재 재화 텍스트
    Text earnText;            // 현재 생산 텍스트
    private int _facNum; // 재화 생산 시설 개수

    private List<Money> _initFac;
    private List<Money> _fac;

    private float[] _initF; // 초기 재화 생산 시설 구매 비용
    private float[] _f;     // 재화 생산 시설 업그레이드 비용
    private int[] _facLevel;  // 재화 생산 시설 레벨

    private float _spoonLevel; // 스푼 업그레이드 비용, 스푼 레벨

    GameObject facScroll, upgScroll; // 스크롤뷰 오브젝트

    FacilityManager fm;

    float timeSpan, checkTime;

    // Use this for initialization
    void Start()
    {
        _money = new Money();

        //_m = 0;
        _click = new Money(1);

        _fac = new List<Money>();
        _fac.Add(new Money(1));
        _fac.Add(new Money(100));
        _fac.Add(new Money(500));
        _fac.Add(new Money(3000));
        _fac.Add(new Money(10000));
        _fac.Add(new Money(40000));
        _fac.Add(new Money(200000));
        _fac.Add(new Money(1700000));
        _fac.Add(new Money(123456789));
        _fac.Add(new Money(4000000000));
        _fac.Add(new Money(75000000000));
        _facNum = _fac.Count;
        /*
        _fac = new float[_facNum];
        _fac[0] = 1;
        _fac[1] = 100;
        _fac[2] = 500;
        _fac[3] = 3000;
        _fac[4] = 10000;
        _fac[5] = 40000;
        _fac[6] = 200000;
        _fac[7] = 1700000;
        _fac[8] = 123456789;
        _fac[9] = 4000000000;
        _fac[10] = 75000000000;*/

        _initFac = new List<Money>();
        //_initFac = new float[_facNum];
        for (int i = 0; i < _facNum; i++)
        {
            _initFac.Add(_fac[i]);
        }

        _facLevel = new int[_facNum];
        for (int j = 0; j < _facNum; j++)
        {
            _facLevel[j] = 0;
        }
        _facLevel[0] = 1;

        moneyText = GameObject.Find("current money").GetComponent<Text>();
        earnText  = GameObject.Find("earning money").GetComponent<Text>();
        facScroll = GameObject.Find("Facility Scroll");
        upgScroll = GameObject.Find("Upgrade Scroll");
        
        _spoonLevel = 1;

        upgScroll.SetActive(false); // 업그레이드 스크롤뷰 비활성화

        fm = GameObject.Find("FacilityManager").GetComponent<FacilityManager>();

        timeSpan = 0.0f;
        checkTime = 1.0f;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (moneyText != null)
        {
            moneyText.text = "Current: " + GetMoney().Print();     //소수 셋째자리에서 버림 표기
        }
        if (earnText != null)
        {
            earnText.text = "Earning: " + GetCurrentEarn().Print();
        }
        timeSpan += Time.deltaTime;
        if (timeSpan > checkTime)
        {
            AutoEarn();
            timeSpan = 0;
        }
	}

    public Money GetMoney()   // 재화 반환
    {
        return _money;
    }
    
    public void SetMoney(Money extra, bool add)  // 재화 수정
    {
        if (add)
        {
            GetMoney().AddMoney(extra);
        }
        else
        {
            if (GetMoney().IsBiggerThan(extra))
            {
                GetMoney().SubMoney(extra);
            }
        }
    }

    public void OnClickButtonMoneyUP()  // 클릭 시 재화 증가
    {
        SetMoney(_click, true);
        //Debug.Log("money + 10, now: " + GetMoney());
    }

    public void OnClickButtonFacility(Button b)  // 재화 생산시설 구매 버튼
    {
        int num;
        if (b.name.Length == 7)
        {
            num = b.name[6] - '1';
        }
        else
        {
            num = b.name[7] - '1' + 10;
        }
        if (GetMoney().IsBiggerThan(_fac[num]))
        {
            Text facText = GameObject.Find("facility" + (num + 1)).GetComponentInChildren<Text>();
            // 구매 여부 확인
            if (!fm.GetIsPurchased()[num])
            {
                fm.newFacObj(num);
                fm.SetIsPurchased(num, true);
                facText.text = facText.text + "\nLv.1";
            }
            // 재화 소모
            SetMoney(_fac[num], false);
            // 구매 가격 증가
            _fac[num] = new Money(_initFac[num].num * Mathf.Pow(1.13f, _facLevel[num]++), _initFac[num].letter1, _initFac[num].letter2);

            // 텍스트 업데이트
            b.GetComponentInChildren<Text>().text = "UPGRADE\n$" + _fac[num].Print(); // 버튼, 소수 셋째 자리에서 버림 표기
            facText.text = facText.text.Remove(facText.text.LastIndexOf(".") + 1) + _facLevel[num]; // 레벨
        }
    }
    public void OnClickButtonMenu(Button b)
    {
        // 하단 메뉴 버튼
        if (b.name == "Facilities Button")
        {
            // 재화생산시설 스크롤뷰 활성화
            upgScroll.SetActive(false);
            facScroll.SetActive(true);
        }
        else
        {
            // 업그레이드 스크롤뷰 활성화
            upgScroll.SetActive(true);
            facScroll.SetActive(false);
        }
    }

    public void OnClickButtonSpoon()
    {
        Money spoon = new Money(50000 * Mathf.Pow(10, _spoonLevel)); // 구매 시 필요한 재화
        if (GetMoney().IsBiggerThan(spoon))
        {
            SetMoney(spoon, false); // 재화 소모
            _spoonLevel++;   // 스푼 레벨 증가

            // 텍스트 업데이트
            Text spoonText = GameObject.Find("spoon").GetComponentInChildren<Text>();
            spoonText.text = "Lv. " + _spoonLevel;
            Text spoonUpg = GameObject.Find("spoon button").GetComponentInChildren<Text>();
            spoon.num = spoon.num * 10;
            spoon.MoneyRule();
            spoonUpg.text = "UPGRADE\n$" + spoon.Print();

            _click.num = _click.num * 2;
            _click.MoneyRule();
        }
    }

    public Money GetCurrentEarn()
    {
        Money earn = new Money();
        int count = 0;
        for (int i = 0; i < 11; i++)
        {
            if (fm.GetIsPurchased()[i])
            {
                count++;
                earn.AddMoney(new Money(fm.facEarn[i] * _facLevel[i]));
            }
        }
        earn.num = earn.num * Mathf.Pow(2, count);
        earn.MoneyRule();
        earn.num = earn.num * 13;
        earn.MoneyRule();
        earn.num = earn.num * Mathf.Pow(2, _spoonLevel - 1);
        earn.MoneyRule();
        return earn;
    }

    public void AutoEarn()
    {
        SetMoney(GetCurrentEarn(), true);
    }
}
