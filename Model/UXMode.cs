using System.ComponentModel;

namespace CanvasPractice.Model
{
    public enum UXMode
    {
        [Description("新增")]
        Draw,

        [Description("刪除")]
        Erase,

        [Description("選擇")]
        Select,
    }
}
