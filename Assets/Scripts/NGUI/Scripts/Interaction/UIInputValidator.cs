using UnityEngine;

/// <summary>
/// Basic input validator with a few presets. I suggest making your own validator if you need new functionality.
/// </summary>
[RequireComponent(typeof(UIInput))]
[AddComponentMenu("NGUI/Interaction/Input Validator")]
public class UIInputValidator : MonoBehaviour
{
    public enum Validation
    {
        None,
        Integer,
        Float,
        Alphanumeric,
        Username,
        Name,
    }

    /// <summary>
    /// Validation logic, choose one of the presets.
    /// </summary>
    public Validation logic;

    /// <summary>
    /// Assign the validator.
    /// </summary>
    private void Start()
    { GetComponent<UIInput>().validator = Validate; }

    /// <summary>
    /// Validate the specified input.
    /// </summary>
    private char Validate(string text, char ch)
    {
        // Validation is disabled
        if (logic == Validation.None || !enabled) return ch;

        if (logic == Validation.Integer)
        {
            // Integer number validation
            if (ch >= '0' && ch <= '9') return ch;
            if (ch == '-' && text.Length == 0) return ch;
        }
        else if (logic == Validation.Float)
        {
            // Floating-point number
            if (ch >= '0' && ch <= '9') return ch;
            if (ch == '-' && text.Length == 0) return ch;
            if (ch == '.' && !text.Contains(".")) return ch;
        }
        else if (logic == Validation.Alphanumeric)
        {
            // All alphanumeric characters
            if (ch >= 'A' && ch <= 'Z') return ch;
            if (ch >= 'a' && ch <= 'z') return ch;
            if (ch >= '0' && ch <= '9') return ch;
        }
        else if (logic == Validation.Username)
        {
            // Lowercase and numbers
            if (ch >= 'A' && ch <= 'Z') return (char)(ch - 'A' + 'a');
            if (ch >= 'a' && ch <= 'z') return ch;
            if (ch >= '0' && ch <= '9') return ch;
        }
        else if (logic == Validation.Name)
        {
            char lastChar = (text.Length > 0) ? text[text.Length - 1] : ' ';

            if (ch >= 'a' && ch <= 'z')
            {
                // Space followed by a letter -- make sure it's capitalized
                if (lastChar == ' ') return (char)(ch - 'a' + 'A');
                return ch;
            }
            else if (ch >= 'A' && ch <= 'Z')
            {
                // Uppercase letters are only allowed after spaces (and apostrophes)
                if (lastChar != ' ' && lastChar != '\'') return (char)(ch - 'A' + 'a');
                return ch;
            }
            else if (ch == '\'')
            {
                // Don't allow more than one apostrophe
                if (lastChar != ' ' && lastChar != '\'' && !text.Contains("'")) return ch;
            }
            else if (ch == ' ')
            {
                // Don't allow more than one space in a row
                if (lastChar != ' ' && lastChar != '\'') return ch;
            }
        }
        return (char)0;
    }
}