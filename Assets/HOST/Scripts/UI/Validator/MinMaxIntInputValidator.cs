using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[CreateAssetMenu(menuName = "HOST/TMPro Validators/Min Max Int Validator")]


public class MinMaxIntInputValidator : TMP_InputValidator
{

    [Serializable]
    struct Limit
    {
        public long value;
        public bool use;
    }
    [SerializeField]
    private Limit min;
    [SerializeField]
    private Limit max;

    public override char Validate(ref string text, ref int pos, char ch)
    {
        if (char.IsDigit(ch))
        {
            string tmp = text.Insert(pos, ch.ToString());
            long value;
            try
            {
                value = long.Parse(tmp);
            }
            catch (Exception)
            {
                return '\0';
            }
            if ((min.use && min.value <= value || !min.use) &&
                (max.use && max.value >= value || !max.use))
            {
                text += ch;
                pos++;
                return ch;
            }
            else if(min.use && min.value > value)
            {
                text = min.value.ToString();
                pos = text.Length;
                return '\0';
            }
            else if(max.use && max.value < value)
            {
                text = max.value.ToString();
                pos = text.Length;
                return '\0';
            }
        }
        return '\0';
    }





}
