using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace CodeGenerator.CustomControl
{
    public class MyTreeView : TreeView
    {
        public MyTreeView()
        {
            this.DrawMode = TreeViewDrawMode.OwnerDrawText;
        }

        protected override void OnDrawNode(DrawTreeNodeEventArgs e)
        {
            //base.OnDrawNode(e);
            var b = Brushes.Black;//默认字体为黑色
            if (e.Node.ForeColor == Color.Gray)
            {
                var p = e.Bounds.Location;//获取节点的位置
                p.X = p.X - 12;//覆盖到默认画CheckBox的位置
                CheckBoxRenderer.DrawCheckBox(e.Graphics, p, CheckBoxState.UncheckedDisabled);//画一个禁用的选中的CheckBox
                b = Brushes.Gray;//当前节点字体为灰色
            }
            if ((e.State & TreeNodeStates.Focused) != 0)
                b = Brushes.White;//点击某节点时节点字体颜色为白色
            e.Graphics.DrawString(e.Node.Text, this.Font, b, e.Bounds.Location);//画文本
        }
    }
}
