using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Newtonsoft.Json.Linq;

namespace MySolution.BotFramework.CustomActions.PBI.Client
{
    public static class JsonExtensions
    {
        public static T EvaluateExpression<T>(this ValueExpression valExpr, object state)
        {
            JToken jtoken = valExpr.ExpressionText == null ?
                JToken.FromObject(valExpr.Value).DeepClone().ReplaceJTokenRecursively(state)
                : valExpr.GetValue(state) as JToken;

            return jtoken.ToObject<T>();
        }
    }
}