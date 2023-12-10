using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FighterUI : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject health_slider;
    public Fighter fighter;
    private Slider _HealthSlider;
    public TextMeshProUGUI nameText;
    void Start()
    {
        fighter = GetComponent<Fighter>();

        if (health_slider != null)
        {
            _HealthSlider = health_slider.GetComponent<Slider>();
            _HealthSlider.minValue = 0;
            if (fighter.maxHealth != -1)
                _HealthSlider.maxValue = fighter.maxHealth;
            else
                _HealthSlider.maxValue = fighter.initialHealth;
            _HealthSlider.value = fighter.initialHealth;
        }
        if (nameText != null)
        {
            nameText.text = fighter.Name;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_HealthSlider != null)
        {
            _HealthSlider.value = fighter.HealthValue;

        }

    }
}
