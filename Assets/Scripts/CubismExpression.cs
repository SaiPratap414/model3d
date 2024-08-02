using Live2D.Cubism.Core;
using Live2D.Cubism.Framework.Expression;
using System.Collections.Generic;
using UnityEngine;


public enum Expressions
{
    Happy, Sad, Surprised, Angry, Confused
}

public class CubismExpression : MonoBehaviour
{
    public static CubismExpression instance;

    // For Testing Now. later we can just assign as-in Enum.
    Dictionary<Expressions, int> ExpressionPairs = new Dictionary<Expressions, int>()
    {
        {Expressions.Happy,1 },
        {Expressions.Sad,2 },
        {Expressions.Surprised,5 },
        {Expressions.Angry,8 },
        {Expressions.Confused,6 },
    };


    CubismExpressionController _expressionController;

    private void Start()
    {
        instance = this;

        var model = this.FindCubismModel();

        _expressionController = model.GetComponent<CubismExpressionController>();
    }

    public void ChangeExpression(Expressions expression)
    {
        if (_expressionController != null)
        {
            _expressionController.CurrentExpressionIndex = ExpressionPairs[expression];
            ChatBoxManager.instance.expressionText.text = expression.ToString();
            Debug.Log("Expression Succuefully set to: " + expression);
        }
    }
}
