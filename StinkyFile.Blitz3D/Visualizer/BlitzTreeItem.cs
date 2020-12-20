using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StinkyFile.Blitz3D.Visualizer
{
    public partial class BlitzTreeVisualizer
    {
        public class BlitzTreeItem
        {
            public BlitzObject Focus;
            public IEnumerable<BlitzObject> Children
            {
                get; private set;
            }
            public bool HasAnimator => Focus.HasAnimator;
            public string Path => "";

            public BlitzTreeItem(IEnumerable<BlitzObject> Source, BlitzObject focusItem)
            {
                Focus = focusItem;
                loadChildren(Source);
            }

            private void loadChildren(IEnumerable<BlitzObject> Source)
            {
                Children = Source.Where(x => x.Parent == this.Focus);
            }

            public static string DisplayTypeName(object value)
            {
                Type valueType = value.GetType();
                return valueType.IsClass ? $"[{valueType.Name}]" : "";
            }

            public IEnumerable<KeyValuePair<string, object>> GetPropertyValues()
            {
                foreach (var param in Focus.GetType().GetProperties(
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.Instance))
                {
                    var att = param.CustomAttributes.FirstOrDefault();
                    if (att == default) continue;
                    if (att.AttributeType != typeof(StinkyFile.Primitive.EditorVisible)) continue;
                    var paramAtt = (param.GetCustomAttributes().ElementAt(0) as Primitive.EditorVisible);
                    var value = param.GetValue(Focus);
                    if (value == null)
                        value = "<not set>";
                    if (value is List<BlitzObject>)
                        value = string.Join(", ", value as List<BlitzObject>);
                    yield return new KeyValuePair<string, object>(param.Name, $"{ DisplayTypeName(value) } {value}");
                }
            }
        }
    }
}
