﻿using System.Linq.Expressions;
using System.Reflection;

namespace AppLauncherMAUI.Utilities;

public abstract class ExtendedBindableObject : BindableObject
{
    public void RaisePropertyChanged<T>(Expression<Func<T>> property)
    {
        var name = GetMemberInfo(property).Name;
        OnPropertyChanged(name);
    }

    private static MemberInfo GetMemberInfo(Expression expression)
    {
        MemberExpression operand;
        LambdaExpression lambdaExpression = (LambdaExpression)expression;
        if (lambdaExpression.Body as UnaryExpression != null)
        {
            UnaryExpression body = (UnaryExpression)lambdaExpression.Body;
            operand = (MemberExpression)body.Operand;
        }
        else
        {
            operand = (MemberExpression)lambdaExpression.Body;
        }
        return operand.Member;
    }
}
